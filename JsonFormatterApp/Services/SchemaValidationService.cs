using System;
using System.Collections.Generic;
using System.Linq;
using NJsonSchema;
using Newtonsoft.Json.Linq;

namespace JsonFormatterApp.Services
{
    public class SchemaValidationService
    {
        /// <summary>
        /// Validate JSON against a schema
        /// </summary>
        public (bool IsValid, List<string> Errors) ValidateAgainstSchema(string jsonText, string schemaText)
        {
            var errors = new List<string>();

            try
            {
                var schema = JsonSchema.FromJsonAsync(schemaText).Result;
                var jToken = JToken.Parse(jsonText);
                var validationErrors = schema.Validate(jToken.ToString());

                if (validationErrors.Any())
                {
                    errors.AddRange(validationErrors.Select(e => $"{e.Path}: {e.Kind} - {e.ToString()}"));
                    return (false, errors);
                }

                return (true, errors);
            }
            catch (Exception ex)
            {
                errors.Add($"Schema validation error: {ex.Message}");
                return (false, errors);
            }
        }

        /// <summary>
        /// Generate JSON schema from sample JSON
        /// </summary>
        public string GenerateSchema(string jsonText)
        {
            try
            {
                var schema = JsonSchema.FromSampleJson(jsonText);
                return schema.ToJson();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to generate schema: {ex.Message}", ex);
            }
        }
    }
}
