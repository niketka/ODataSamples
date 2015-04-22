using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Library;
using Microsoft.OData.Edm.Library.Annotations;
using Microsoft.OData.Edm.Library.Expressions;
using Microsoft.OData.Edm.Library.Values;

namespace EdmLibSample
{
    public class SampleModelBuilder
    {
        private readonly EdmModel _model = new EdmModel();
        private EdmComplexType _addressType;
        private EdmComplexType _workAddressType;
        private EdmEnumType _categoryType;
        private EdmEntityType _customerType;
        private EdmEntityType _orderType;
        private EdmEntityType _urgentOrderType;
        private EdmNavigationProperty _friendsProperty;
        private EdmNavigationProperty _purchasesProperty;
        private EdmNavigationProperty _intentionsProperty;
        private EdmAction _rateAction;
        private EdmFunction _mostExpensiveFunction;
        private EdmEntityContainer _defaultContainer;
        private EdmEntitySet _customerSet;
        private EdmEntitySet _orderSet;
        private EdmSingleton _vipCustomer;
        private EdmFunctionImport _mostValuableFunctionImport;
        public SampleModelBuilder BuildAddressType()
        {
            _addressType = new EdmComplexType("Sample.NS", "Address");
            _addressType.AddStructuralProperty("Street", EdmPrimitiveTypeKind.String);
            _addressType.AddStructuralProperty("City", EdmPrimitiveTypeKind.String);
            _addressType.AddStructuralProperty("Postcode", EdmPrimitiveTypeKind.Int32);
            _addressType.AddStructuralProperty("GeometryLoc", EdmPrimitiveTypeKind.GeometryPoint);
            _addressType.AddStructuralProperty("GeographyLoc", new EdmSpatialTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeographyPoint), true, 1234));
            _model.AddElement(_addressType);
            return this;
        }
        public SampleModelBuilder BuildWorkAddressType()
        {
            _workAddressType = new EdmComplexType("Sample.NS", "WorkAddress", _addressType);
            _workAddressType.AddStructuralProperty("Company", EdmPrimitiveTypeKind.String);
            _model.AddElement(_workAddressType);
            return this;
        }
        public SampleModelBuilder BuildCategoryType()
        {
            _categoryType = new EdmEnumType("Sample.NS", "Category", EdmPrimitiveTypeKind.Int64, isFlags: true);
            _categoryType.AddMember("Books", new EdmIntegerConstant(1L));
            _categoryType.AddMember("Dresses", new EdmIntegerConstant(2L));
            _categoryType.AddMember("Sports", new EdmIntegerConstant(4L));
            _model.AddElement(_categoryType);
            return this;
        }
        public SampleModelBuilder BuildCustomerType()
        {
            _customerType = new EdmEntityType("Sample.NS", "Customer");
            _customerType.AddKeys(_customerType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            _customerType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String, false);
            _customerType.AddStructuralProperty("Credits",
                new EdmCollectionTypeReference(new EdmCollectionType(EdmCoreModel.Instance.GetInt64(isNullable: true))));
            _customerType.AddStructuralProperty("Interests", new EdmEnumTypeReference(_categoryType, isNullable: true));
            _customerType.AddStructuralProperty("Address", new EdmComplexTypeReference(_addressType, isNullable: false));
            _friendsProperty = _customerType.AddUnidirectionalNavigation(
                new EdmNavigationPropertyInfo
                {
                    ContainsTarget = false,
                    Name = "Friends",
                    Target = _customerType,
                    TargetMultiplicity = EdmMultiplicity.Many
                });
            _purchasesProperty = _customerType.AddUnidirectionalNavigation(
                new EdmNavigationPropertyInfo
                {
                    ContainsTarget = false,
                    Name = "Purchases",
                    Target = _orderType,
                    TargetMultiplicity = EdmMultiplicity.Many
                });
            _intentionsProperty = _customerType.AddUnidirectionalNavigation(
                new EdmNavigationPropertyInfo
                {
                    ContainsTarget = true,
                    Name = "Intentions",
                    Target = _orderType,
                    TargetMultiplicity = EdmMultiplicity.Many
                });
            _model.AddElement(_customerType);
            return this;
        }
        public SampleModelBuilder BuildOrderType()
        {
            _orderType = new EdmEntityType("Sample.NS", "Order");
            _orderType.AddKeys(_orderType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            _orderType.AddStructuralProperty("Price", EdmPrimitiveTypeKind.Decimal);
            _model.AddElement(_orderType);
            return this;
        }
        public SampleModelBuilder BuildUrgentOrderType()
        {
            _urgentOrderType = new EdmEntityType("Sample.NS", "UrgentOrder", _orderType);
            _urgentOrderType.AddStructuralProperty("Deadline", EdmPrimitiveTypeKind.Date);
            _model.AddElement(_urgentOrderType);
            return this;
        }
        public SampleModelBuilder BuildRateAction()
        {
            _rateAction = new EdmAction("Sample.NS", "Rate",
                returnType: null, isBound: true, entitySetPathExpression: null);
            _rateAction.AddParameter("customer", new EdmEntityTypeReference(_customerType, false));
            _rateAction.AddParameter("rating", EdmCoreModel.Instance.GetInt32(false));
            _model.AddElement(_rateAction);
            return this;
        }
        public SampleModelBuilder BuildMostExpensiveFunction()
        {
            _mostExpensiveFunction = new EdmFunction("Sample.NS", "MostExpensive",
                new EdmEntityTypeReference(_orderType, true), isBound: false, entitySetPathExpression: null, isComposable: true);
            _model.AddElement(_mostExpensiveFunction);
            return this;
        }
        public SampleModelBuilder BuildDefaultContainer()
        {
            _defaultContainer = new EdmEntityContainer("Sample.NS", "DefaultContainer");
            _model.AddElement(_defaultContainer);
            return this;
        }
        public SampleModelBuilder BuildCustomerSet()
        {
            _customerSet = _defaultContainer.AddEntitySet("Customers", _customerType);
            _customerSet.AddNavigationTarget(_friendsProperty, _customerSet);
            _customerSet.AddNavigationTarget(_purchasesProperty, _orderSet);
            return this;
        }
        public SampleModelBuilder BuildOrderSet()
        {
            _orderSet = _defaultContainer.AddEntitySet("Orders", _orderType);
            return this;
        }
        public SampleModelBuilder BuildVipCustomer()
        {
            _vipCustomer = _defaultContainer.AddSingleton("VipCustomer", _customerType);
            return this;
        }
        public SampleModelBuilder BuildMostValuableFunctionImport()
        {
            _mostValuableFunctionImport = _defaultContainer.AddFunctionImport("MostValuable", _mostExpensiveFunction, new EdmEntitySetReferenceExpression(_orderSet));
            return this;
        }
        public SampleModelBuilder BuildAnnotations()
        {
            var term1 = new EdmTerm("Sample.NS", "MaxCount", EdmCoreModel.Instance.GetInt32(true));
            var annotation1 = new EdmAnnotation(_customerSet, term1, new EdmIntegerConstant(10000000L));
            _model.AddVocabularyAnnotation(annotation1);
            var term2 = new EdmTerm("Sample.NS", "KeyName", EdmCoreModel.Instance.GetString(true));
            var annotation2 = new EdmAnnotation(_customerType, term2, new EdmStringConstant("Id"));
            annotation2.SetSerializationLocation(_model, EdmVocabularyAnnotationSerializationLocation.Inline);
            _model.AddVocabularyAnnotation(annotation2);
            var term3 = new EdmTerm("Sample.NS", "Width", EdmCoreModel.Instance.GetInt32(true));
            var annotation3 = new EdmAnnotation(_customerType.FindProperty("Name"), term3, new EdmIntegerConstant(10L));
            annotation3.SetSerializationLocation(_model, EdmVocabularyAnnotationSerializationLocation.Inline);
            _model.AddVocabularyAnnotation(annotation3);
            return this;
        }
        public IEdmModel GetModel()
        {
            return _model;
        }
    }
}
