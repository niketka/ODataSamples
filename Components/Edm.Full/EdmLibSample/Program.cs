using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Annotations;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Values;
using Microsoft.OData.Edm.Validation;

namespace EdmLibSample
{
    class Program
    {
        public static void Main(string[] args)
        {
            var builder = new SampleModelBuilder();
            var model = builder
                .BuildAddressType()
                .BuildWorkAddressType()
                .BuildCategoryType()
                .BuildOrderType()
                .BuildUrgentOrderType()
                .BuildCustomerType()
                .BuildRateAction()
                .BuildMostExpensiveFunction()
                .BuildDefaultContainer()
                .BuildOrderSet()
                .BuildCustomerSet()
                .BuildVipCustomer()
                .BuildMostValuableFunctionImport()
                .BuildAnnotations()
                .GetModel();
            WriteModelToCsdl(model, "csdl.xml");
            TestExtensionMethods(model);
            var model1 = ReadModel("csdl.xml");
            WriteModelToCsdl(model1, "csdl1.xml");
        }
        private static void WriteModelToCsdl(IEdmModel model, string fileName)
        {
            using (var writer = XmlWriter.Create(fileName))
            {
                IEnumerable<EdmError> errors;
                model.TryWriteCsdl(writer, out errors);
            }
        }
        private static IEdmModel ReadModel(string fileName)
        {
            using (var reader = XmlReader.Create(fileName))
            {
                IEdmModel model;
                IEnumerable<EdmError> errors;
                if (CsdlReader.TryParse(new[] { reader }, out model, out errors))
                {
                    return model;
                }
                return null;
            }
        }
        private static void TestExtensionMethods(IEdmModel model)
        {
            // Find an entity set.
            var customerSet = model.FindDeclaredEntitySet("Customers");
            Console.WriteLine("{0} '{1}' found.", customerSet.NavigationSourceKind(), customerSet.Name);
            // Find any kind of navigation source (entity set or singleton).
            var vipCustomer = model.FindDeclaredNavigationSource("VipCustomer");
            Console.WriteLine("{0} '{1}' found.", vipCustomer.NavigationSourceKind(), vipCustomer.Name);
            // Find a type (complex or entity or enum).
            var orderType = model.FindDeclaredType("Sample.NS.Order");
            Console.WriteLine("{0} type '{1}' found.", orderType.TypeKind, orderType.FullName());
            var addressType = model.FindDeclaredType("Sample.NS.Address");
            Console.WriteLine("{0} type '{1}' found.", addressType.TypeKind, addressType);
            // Find derived type of some type.
            var workAddressType = model.FindAllDerivedTypes((IEdmStructuredType)addressType).Single();
            Console.WriteLine("Type '{0}' is the derived from '{1}'.", ((IEdmSchemaType)workAddressType).Name, addressType.Name);
            // Find an operation.
            var rateAction = model.FindDeclaredOperations("Sample.NS.Rate").Single();
            Console.WriteLine("{0} '{1}' found.", rateAction.SchemaElementKind, rateAction.Name);
            // Find an operation import.
            var mostValuableFunctionImport = model.FindDeclaredOperationImports("MostValuable").Single();
            Console.WriteLine("{0} '{1}' found.", mostValuableFunctionImport.ContainerElementKind, mostValuableFunctionImport.Name);
            // Find an annotation and get its value.
            var maxCountAnnotation = (IEdmValueAnnotation)model.FindDeclaredVocabularyAnnotations(customerSet).Single();
            var maxCountValue = ((IEdmIntegerValue)maxCountAnnotation.Value).Value;
            Console.WriteLine("'{0}' = '{1}' on '{2}'", maxCountAnnotation.Term.Name, maxCountValue, ((IEdmEntitySet)maxCountAnnotation.Target).Name);
        }
    }
}
