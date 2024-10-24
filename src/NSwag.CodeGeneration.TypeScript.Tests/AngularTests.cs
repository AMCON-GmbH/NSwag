using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NJsonSchema;
using NJsonSchema.NewtonsoftJson.Generation;
using NSwag.Generation.WebApi;
using Xunit;

namespace NSwag.CodeGeneration.TypeScript.Tests
{
    public class AngularTests
    {
        public class Foo
        {
            public string Bar { get; set; }
        }

        [Route("[controller]/[action]")]
        public class DiscussionController : Controller
        {
            [HttpPost]
            public void AddMessage([FromBody, Required] Foo message)
            {
            }

            [HttpPost]
            public void GenericRequestTest1(GenericRequest1 request)
            {
            }

            [HttpPost]
            public void GenericRequestTest2(GenericRequest2 request)
            {
            }
        }

        [Route("[controller]/[action]")]
        public class ComplexController : Controller
        {
            [HttpPost]
            [ProducesResponseType(typeof(Foo),200), ProducesResponseType(204)]
            public IActionResult RequestWithMultipleSuccess([FromBody, Required] Foo message)
            {
                if (message.Bar != null)
                    return Json(message);

                return NoContent();
            }
        }

        public class GenericRequestBase<T>
            where T : RequestBodyBase
        {
            [Required] public T Request { get; set; }
        }

        public class RequestBodyBase
        {
        }

        public class RequestBody : RequestBodyBase
        {
        }

        public class GenericRequest1 : GenericRequestBase<RequestBodyBase>
        {
        }

        public class GenericRequest2 : GenericRequestBase<RequestBody>
        {
        }

        public class UrlEncodedRequestConsumingController : Controller
        {
            [HttpPost]
            [Consumes("application/x-www-form-urlencoded")]
            public void AddMessage([FromForm] Foo message, [FromForm] string messageId)
            {
            }
        }

        [Fact]
        public async Task When_return_value_is_void_then_client_returns_observable_of_void()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings
            {
                SchemaSettings = new NewtonsoftJsonSchemaGeneratorSettings { SchemaType = SchemaType.Swagger2 }
            });
            var document = await generator.GenerateForControllerAsync<DiscussionController>();
            var json = document.ToJson();

            // Act
            var codeGen = new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings
            {
                Template = TypeScriptTemplate.Angular,
                GenerateClientInterfaces = true,
                TypeScriptGeneratorSettings =
                {
                    TypeScriptVersion = 2.0m
                }
            });
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("addMessage(message: Foo): Observable<void>", code);
        }

        [Fact]
        public async Task When_multiple_responses_are_supported_then_client_treats_them_as_successful()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings
            {
                SchemaSettings = new NewtonsoftJsonSchemaGeneratorSettings { SchemaType = SchemaType.OpenApi3 }
            });
            var document = await generator.GenerateForControllerAsync<ComplexController>();

            // Act
            var codeGen = new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings
            {
                Template = TypeScriptTemplate.Angular,
                GenerateClientInterfaces = true,
                RxJsVersion = 7.8m,
                TypeScriptGeneratorSettings =
                {
                    TypeScriptVersion = 5.0m
                }
            });
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("else if (status === 204) {\n            return blobToText(responseBlob).pipe(_observableMergeMap((_responseText: string) => {\n            return _observableOf(null as any);\n            }));\n        }", code);
        }

        [Fact]
        public async Task When_export_types_is_true_then_add_export_before_classes()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings
            {
                SchemaSettings = new NewtonsoftJsonSchemaGeneratorSettings { SchemaType = SchemaType.Swagger2 }
            });
            var document = await generator.GenerateForControllerAsync<DiscussionController>();
            var json = document.ToJson();

            // Act
            var codeGen = new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings
            {
                Template = TypeScriptTemplate.Angular,
                GenerateClientInterfaces = true,
                TypeScriptGeneratorSettings =
                {
                    TypeScriptVersion = 2.0m,
                    ExportTypes = true
                }
            });
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("export class DiscussionClient", code);
            Assert.Contains("export interface IDiscussionClient", code);
        }

        [Fact]
        public async Task When_export_types_is_false_then_dont_add_export_before_classes()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings
            {
                SchemaSettings = new NewtonsoftJsonSchemaGeneratorSettings { SchemaType = SchemaType.Swagger2 }
            });
            var document = await generator.GenerateForControllerAsync<DiscussionController>();
            var json = document.ToJson();

            // Act
            var codeGen = new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings
            {
                Template = TypeScriptTemplate.Angular,
                GenerateClientInterfaces = true,
                TypeScriptGeneratorSettings =
                {
                    TypeScriptVersion = 2.0m,
                    ExportTypes = false
                }
            });
            var code = codeGen.GenerateFile();

            // Assert
            Assert.DoesNotContain("export class DiscussionClient", code);
            Assert.DoesNotContain("export interface IDiscussionClient", code);
        }

        [Fact]
        public async Task When_generic_request()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings
            {
                SchemaSettings = new NewtonsoftJsonSchemaGeneratorSettings { SchemaType = SchemaType.Swagger2 }
            });
            var document = await generator.GenerateForControllerAsync<DiscussionController>();
            var json = document.ToJson();

            // Act
            var codeGen = new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings
            {
                Template = TypeScriptTemplate.Angular,
                GenerateDtoTypes = true,
                TypeScriptGeneratorSettings =
                {
                    TypeScriptVersion = 2.7m,
                    ExportTypes = false
                }
            });
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("this.request = new RequestBodyBase()", code);
            Assert.Contains("this.request = new RequestBody()", code);
        }

        [Fact]
        public async Task When_consumes_is_url_encoded_then_construct_url_encoded_request()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings
            {
                SchemaSettings = new NewtonsoftJsonSchemaGeneratorSettings { SchemaType = SchemaType.Swagger2 }
            });
            var document = await generator.GenerateForControllerAsync<UrlEncodedRequestConsumingController>();
            var json = document.ToJson();

            // Act
            var codeGen = new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings
            {
                Template = TypeScriptTemplate.Angular,
                TypeScriptGeneratorSettings =
                {
                    TypeScriptVersion = 2.0m
                }
            });
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("content_", code);
            Assert.DoesNotContain("FormData", code);
            Assert.Contains("\"Content-Type\": \"application/x-www-form-urlencoded\"", code);
        }
    }
}