using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.Security;
using NSubstitute;
using NUnit.Framework;
using Zone.Epi.ContentCopy.Core.Services;
using Zone.Epi.ContentCopy.Models;

namespace Zone.Epi.ContentCopy.Tests
{
	[TestFixture]
	public class PropertyServiceTests
	{
		private PropertyService _propertyService;

		private IContentLoader _stubContentLoader;
		private IContentRepository _stubContentRepository;
		private IContentVersionRepository _stubContentVersionRepository;

		private const string testLanguageCode = "en";

		private const string firstProperty = "First Property";
		private const string secondProperty = "SecondProperty";

		private const int sourcePageId = 1;
		private const int destinationPageId = 2;

		[SetUp]
		public void SetUp()
		{
			_stubContentLoader = Substitute.For<IContentLoader>();
			_stubContentRepository = Substitute.For<IContentRepository>();
			_stubContentVersionRepository = Substitute.For<IContentVersionRepository>();
			_propertyService = new PropertyService(_stubContentLoader, _stubContentRepository, _stubContentVersionRepository);
		}

		[Test]
		public void GetContentDetail_ValidItemId_ReturnsContentDetailItem()
		{
			// Arrange
			var testLanguage = CultureInfo.GetCultureInfo(testLanguageCode);

			const int itemId = 123;
			const string detailPageName = "Detail Page";
			const string sampleField = "Sample Field 1";
			const string otherFieldName = "Other Field";
			var detailPageReference = new ContentReference(itemId);
			var detailPage = new PageData
			{
				Property =
				{
					new PropertyString {Name = "PageName", Value = detailPageName},
					new PropertyString {Name = sampleField, Value = "Some value", IsPropertyData  = true},
					new PropertyContentReference {Name = "PageLink", Value = detailPageReference},
					new PropertyString {Name = otherFieldName, Value = "Some other value", IsPropertyData = true, IsLanguageSpecific = true}
				}
			};

			_stubContentLoader.Get<PageData>(detailPageReference, testLanguage).Returns(detailPage);

			// Act
			var resultContentSpecific = _propertyService.GetContentDetail(itemId, testLanguageCode, true);
			var resultAllProperties = _propertyService.GetContentDetail(itemId, testLanguageCode, false);

			// Assert
			Assert.AreEqual(itemId, resultContentSpecific.Id);
			Assert.AreEqual(otherFieldName, resultContentSpecific.Properties.First());
			Assert.AreEqual(1, resultContentSpecific.Properties.Count());

			Assert.AreEqual(itemId, resultAllProperties.Id);
			Assert.AreEqual(detailPageName, resultAllProperties.Text);
			Assert.AreEqual(otherFieldName, resultAllProperties.Properties.First());
			Assert.AreEqual(sampleField, resultAllProperties.Properties.Last());
			Assert.AreEqual(2, resultAllProperties.Properties.Count());
		}

		[Test]
		public void CopyContentProperties_SameSourceAndDestination_ReturnsFailureStatus()
		{
			// Arrange
			var request = new ContentCopyRequestDetail
			{
				Source = 1,
				Destination = 1
			};

			// Act
			var result = _propertyService.CopyContentProperties(request);

			// Assert
			Assert.IsFalse(result.Success);
		}

		[Test]
		public void CopyContentProperties_NoPropertiesToCopy_ReturnsFailureStatus()
		{
			// Arrange
			var request = new ContentCopyRequestDetail
			{
				Source = 1,
				Destination = 2
			};

			// Act
			var result = _propertyService.CopyContentProperties(request);

			// Assert
			Assert.IsFalse(result.Success);
		}

		[Test]
		public void CopyContentProperties_OutstandingDraftAndNotInOverwriteMode_ReturnsFailureStatus()
		{
			// Arrange
			var testLanguage = CultureInfo.GetCultureInfo(testLanguageCode);

			var sourcePageReference = new ContentReference(sourcePageId);
			var destinationPageReference = new ContentReference(destinationPageId);

			var request = new ContentCopyRequestDetail
			{
				Source = sourcePageId,
				Destination = destinationPageId,
				Properties = new List<string> { "SampleProperty", "Other Property" },
				CultureCode = testLanguageCode
			};

			var pendingCommonDraft = new ContentVersion(destinationPageReference, "draft", VersionStatus.CheckedIn, DateTime.Now,
				"test", "sample user", destinationPageId, testLanguageCode, true, true);

			_stubContentVersionRepository.LoadCommonDraft(destinationPageReference, testLanguageCode).Returns(pendingCommonDraft);

			_stubContentLoader.Get<PageData>(sourcePageReference, testLanguage).Returns(new PageData());
			_stubContentLoader.Get<PageData>(destinationPageReference, testLanguage).Returns(new PageData());

			// Act
			var result = _propertyService.CopyContentProperties(request);

			// Assert
			Assert.IsFalse(result.Success);
		}

		[Test]
		public void CopyContentProperties_PropertyNotAvailableOnDestination_ReturnsFailureStatus()
		{
			// Arrange
			var testLanguage = CultureInfo.GetCultureInfo(testLanguageCode);

			// Source page setup
			var sourcePageReference = new ContentReference(sourcePageId);

			var sourcePage = new PageData
			{
				Property =
				{
					new PropertyString {Name = "PageName", Value = "Source Page"},
					new PropertyContentReference {Name = "PageLink", Value = sourcePageReference},

					new PropertyString {Name = firstProperty, Value = "Some value", IsPropertyData  = true,  IsLanguageSpecific = true}
				}
			};

			_stubContentLoader.Get<PageData>(sourcePageReference, testLanguage).Returns(sourcePage);

			// Destination page setup
			var destinationPageReference = new ContentReference(destinationPageId);

			var destinationPage = new PageData
			{
				Property =
				{
					new PropertyString {Name = "PageName", Value = "Destination Page"},
					new PropertyContentReference {Name = "PageLink", Value = destinationPageReference}
				}
			};

			_stubContentLoader.Get<PageData>(destinationPageReference, testLanguage).Returns(destinationPage);

			// Copy request setup
			var request = new ContentCopyRequestDetail
			{
				Source = sourcePageId,
				Destination = destinationPageId,
				Properties = new List<string> { firstProperty, secondProperty },
				CultureCode = testLanguageCode
			};

			// Draft setup
			var pendingCommonDraft = new ContentVersion(destinationPageReference, "draft", VersionStatus.Published, DateTime.Now,
				"test", "sample user", destinationPageId, testLanguageCode, true, true);

			_stubContentVersionRepository.LoadCommonDraft(destinationPageReference, testLanguageCode).Returns(pendingCommonDraft);

			// Act
			var result = _propertyService.CopyContentProperties(request);

			// Assert
			Assert.IsFalse(result.Success);
		}

		[Test]
		public void CopyContentProperties_PropertyTypeMismatch_ReturnsFailureStatus()
		{
			// Arrange
			var testLanguage = CultureInfo.GetCultureInfo(testLanguageCode);

			// Source page setup
			var sourcePageReference = new ContentReference(sourcePageId);

			var sourcePage = new PageData
			{
				Property =
				{
					new PropertyString {Name = "PageName", Value = "Source Page"},
					new PropertyContentReference {Name = "PageLink", Value = sourcePageReference},

					new PropertyString {Name = firstProperty, Value = "Some value", IsPropertyData  = true,  IsLanguageSpecific = true}
				}
			};

			_stubContentLoader.Get<PageData>(sourcePageReference, testLanguage).Returns(sourcePage);

			// Destination page setup
			var destinationPageReference = new ContentReference(destinationPageId);

			var destinationPage = new PageData
			{
				Property =
				{
					new PropertyString {Name = "PageName", Value = "Destination Page"},
					new PropertyContentReference {Name = "PageLink", Value = destinationPageReference},

					new PropertyNumber {Name = firstProperty, Value = 12345, IsPropertyData  = true,  IsLanguageSpecific = true}
				}
			};

			_stubContentLoader.Get<PageData>(destinationPageReference, testLanguage).Returns(destinationPage);

			// Copy request setup
			var request = new ContentCopyRequestDetail
			{
				Source = sourcePageId,
				Destination = destinationPageId,
				Properties = new List<string> { firstProperty, secondProperty },
				CultureCode = testLanguageCode
			};

			// Draft setup
			var pendingCommonDraft = new ContentVersion(destinationPageReference, "draft", VersionStatus.Published, DateTime.Now,
				"test", "sample user", destinationPageId, testLanguageCode, true, true);

			_stubContentVersionRepository.LoadCommonDraft(destinationPageReference, testLanguageCode).Returns(pendingCommonDraft);

			// Act
			var result = _propertyService.CopyContentProperties(request);

			// Assert
			Assert.IsFalse(result.Success);
		}

		[Test]
		public void CopyContentProperties_ValidInput_ReturnsSuccessCopyStatus()
		{
			// Arrange
			var testLanguage = CultureInfo.GetCultureInfo(testLanguageCode);

			// Source page setup
			var sourcePageReference = new ContentReference(sourcePageId);

			const string firstSourceValue = "Some source value";
			const string secondSourceValue = "other source value";

			var sourcePage = new PageData
			{
				Property =
				{
					new PropertyString {Name = "PageName", Value = "Source Page"},
					new PropertyContentReference {Name = "PageLink", Value = sourcePageReference},

					new PropertyString {Name = firstProperty, Value = firstSourceValue, IsPropertyData  = true,  IsLanguageSpecific = true},
					new PropertyString {Name = secondProperty, Value = secondSourceValue, IsPropertyData  = true}
				}
			};

			_stubContentLoader.Get<PageData>(sourcePageReference, testLanguage).Returns(sourcePage);

			// Destination page setup
			var destinationPageReference = new ContentReference(destinationPageId);

			var destinationPage = new PageData
			{
				Property =
				{
					new PropertyString {Name = "PageName", Value = "Destination Page"},
					new PropertyContentReference {Name = "PageLink", Value = destinationPageReference},

					new PropertyString {Name = firstProperty, Value = "", IsPropertyData  = true,  IsLanguageSpecific = true},
					new PropertyString {Name = secondProperty, Value = "", IsPropertyData  = true}
				}
			};

			_stubContentLoader.Get<PageData>(destinationPageReference, testLanguage).Returns(destinationPage);

			// Copy request setup
			var request = new ContentCopyRequestDetail
			{
				Source = sourcePageId,
				Destination = destinationPageId,
				Properties = new List<string> { firstProperty, secondProperty },
				CultureCode = testLanguageCode
			};

			// Draft setup
			var pendingCommonDraft = new ContentVersion(destinationPageReference, "draft", VersionStatus.Published, DateTime.Now,
				"test", "sample user", destinationPageId, testLanguageCode, true, true);

			_stubContentVersionRepository.LoadCommonDraft(destinationPageReference, testLanguageCode).Returns(pendingCommonDraft);

			_stubContentRepository.Save(Arg.Any<PageData>(), SaveAction.CheckOut, AccessLevel.Edit).Returns(destinationPageReference);
			// Act
			var result = _propertyService.CopyContentProperties(request);

			// Assert
			Assert.IsTrue(result.Success);
		}
	}
}