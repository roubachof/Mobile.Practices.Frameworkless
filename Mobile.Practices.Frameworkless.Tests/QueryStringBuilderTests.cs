using System;
using NUnit.Framework;
using SeLoger.Mobile.ToolKit.Net;

namespace Mobile.Practices.Frameworkless.Tests
{
	[TestFixture]
	public class QueryStringBuilderTests
	{
		[Test]
		public void TestCase()
		{
			var testSearchDefinition = new TestSearchDefinition();

			testSearchDefinition.ZipCodes = new[] { "75018", "75017", "75016" };
			testSearchDefinition.Search = "searchQuery";
			testSearchDefinition.Type = 1;

            var queryString = testSearchDefinition.ToQueryString();

			Assert.AreEqual("?cp=75018%2C75017%2C75016&search=searchQuery&type=1", queryString);
		}

		[Test]
		public void ShouldThrowOnNullRequiredField()
		{
			var requiredDefinition = new RequiredDefinition();

			var ex = Assert.Throws<ArgumentException>(() => requiredDefinition.ToQueryString());
			Assert.That(ex.Message, Is.EqualTo("Un paramètre obligatoire ne possède pas de valeur.\nParameter name: RequiredField"));
			Assert.That(ex.ParamName, Is.EqualTo("RequiredField"));
		}

		[Test]
		public void ShouldNotUseFieldWithoutAttribute()
		{
			var missingAttributeDefinition = new AttributeMissingDefinition();
			var queryString = missingAttributeDefinition.ToQueryString();
			Assert.That(queryString, Is.Empty);
		}

		[Test]
		public void ShouldNotUseNullField()
		{
			var testSearchDefinition = new TestSearchDefinition();

			testSearchDefinition.Search = "searchQuery";
			testSearchDefinition.Type = 1;

			var queryString = testSearchDefinition.ToQueryString();

			Assert.AreEqual("?search=searchQuery&type=1", queryString);
		}

		[Test]
		public void ShouldThrowIfConverterMismatchFieldType()
		{
			var convertibleDefinition = new ConvertibleDefinition();
			convertibleDefinition.RequiredTypeToConvert = new ConvertibleType();

			var ex = Assert.Throws<ArgumentException>(() => convertibleDefinition.ToQueryString());
			Assert.That(ex.Message, Is.EqualTo($"Un paramètre obligatoire {nameof(convertibleDefinition.RequiredTypeToConvert)} ne peut être converti par le converter {typeof(DummyConverter)}."));
		}

		[Test]
		public void ShouldThrowIfFieldTypeMismatchConverter()
		{
			var convertibleDefinition = new ObjectConvertibleDefinition();
			convertibleDefinition.RequiredObjectToConvert = new object();

			var ex = Assert.Throws<ArgumentException>(() => convertibleDefinition.ToQueryString());
			Assert.That(ex.Message, Is.EqualTo($"Un paramètre obligatoire {nameof(convertibleDefinition.RequiredObjectToConvert)} ne peut être converti par le converter {typeof(ConvertibleTypeConverter)}."));
		}
	}
	public class ConvertibleDefinition : QueryStringBuilder
	{
		[QueryStringParameter("typeToConvert", typeof(ConvertibleTypeConverter))]
		public ConvertibleType TypeToConvert { get; set; }

		[QueryStringParameter("requiredTypeToConvert", true, typeof(DummyConverter))]
		public ConvertibleType RequiredTypeToConvert { get; set; }
	}

	public class ObjectConvertibleDefinition : QueryStringBuilder
	{
		[QueryStringParameter("requiredObjectTypeToConvert", true, typeof(ConvertibleTypeConverter))]
		public object RequiredObjectToConvert { get; set; }
	}

	public class ObjectConfvertibleDefinition : QueryStringBuilder
	{
		[QueryStringParameter("requiredObjectTypeToConvert", true, typeof(Nullable))]
		public object RequiredObjectToConvert { get; set; }
	}

	public class ConvertibleTypeConverter : IQueryStringConverter
	{
		public bool CanConvert(Type type)
		{
			return type == typeof(ConvertibleType);
		}

		public string Convert(object obj)
		{
			return "converted";
		}
	}

	public class DummyConverter : IQueryStringConverter
	{
		public bool CanConvert(Type t)
		{
			return false;
		}

		public string Convert(object o)
		{
			return string.Empty;
		}
	}

	public class ConvertibleType { }

	public class AttributeMissingDefinition : QueryStringBuilder
	{
		public string AttributeMissingForField { get; set; }
	}

	public class RequiredDefinition : QueryStringBuilder
	{
		[QueryStringParameter("req", true)]
		public string RequiredField { get; set; }
	}

	public class TestSearchDefinition : QueryStringBuilder
	{
		[QueryStringParameter("cp")]
		public string[] ZipCodes { get; set; }

		[QueryStringParameter("search")]
		public string Search { get; set; }

		[QueryStringParameter("type")]
		public int? Type { get; set; }
	}
}