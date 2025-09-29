# Application Building Blocks

This folder contains core application-level building blocks that provide essential functionality for building robust applications. These components handle common concerns such as concurrency, timing, data management, serialization, and more.

## Core Components

### Concurrency & Threading
- **[ConcurrentStringBuilder](ConcurrentStringBuilder.md)** - Thread-safe string builder for high-performance concurrent scenarios
- **[DispatcherTimer](DispatcherTimer.md)** - Enhanced timer implementation with dispatcher support

### Data Management & Tracking
- **[FeederMessage](FeederMessage.md)** - Standardized message structure for data feeding operations
- **[Telemetry](Telemetry.md)** - Comprehensive telemetry and monitoring capabilities

### Exception Handling
- **[ExceptionInfo](ExceptionInfo.md)** - Detailed exception information and metadata
- **[InconvertibleException](InconvertibleException.md)** - Exception for type conversion failures

### Type System Extensions
- **[ICloneable](ICloneable.md)** - Enhanced cloning interface with deep copy support
- **[IConvertible](IConvertible.md)** - Extended type conversion capabilities

### Configuration
- **[ServiceConfiguration](ServiceConfiguration.md)** - Service configuration management and validation

## Specialized Components

### Attributes
The `Attributes/` folder contains custom attributes for metadata and serialization:
- **[IgnoreMemberAttribute](Attributes/IgnoreMemberAttribute.md)** - Attribute to exclude members from processing
- **[JsonSerializationAttribute](Attributes/JsonSerializationAttribute.md)** - Custom JSON serialization control

### Certificate Management
The `Certificate/` folder provides certificate handling:
- **[CertificateModel](Certificate/CertificateModel.md)** - Certificate model for security operations

### Change Tracking
The `ChangeTrackingItems/` folder offers comprehensive change tracking capabilities:
- **[ChangeTrackingItem](ChangeTrackingItems/ChangeTrackingItem.md)** - Individual item change tracking
- **[ChangeTrackingItemCollection](ChangeTrackingItems/ChangeTrackingItemCollection.md)** - Collection-level change tracking
- **[ChangeTrackingObject](ChangeTrackingItems/ChangeTrackingObject.md)** - Object-level change tracking
- **[ChangeTrackingObjectAdapter](ChangeTrackingItems/ChangeTrackingObjectAdapter.md)** - Adapter for existing objects
- **[ChangeType](ChangeTrackingItems/ChangeType.md)** - Enumeration of change types
- **[README](ChangeTrackingItems/README.md)** - Detailed documentation for change tracking

### Cryptography & Security
The `Ciphering/` folder provides encryption and security utilities:
- **[EncryptionService](Ciphering/EncryptionService.md)** - General encryption service interface
- **[PasswordGenerator](Ciphering/PasswordGenerator.md)** - Secure password generation utilities
- **[RsaEncryptionService](Ciphering/RsaEncryptionService.md)** - RSA encryption implementation
- **[README](Ciphering/README.md)** - Cryptography overview and usage

### Collections
The `Collections/` folder contains specialized collection types:
- **[BindingDictionary](Collections/BindingDictionary.md)** - Dictionary with data binding support
- **[GenericOrderedDictionary](Collections/GenericOrderedDictionary.md)** - Ordered dictionary implementation
- **[LinkedArray](Collections/LinkedArray.md)** - Linked array data structure
- **[README](Collections/README.md)** - Collections overview and usage patterns

### Correlation Support
The `CorrelationId/` folder provides request correlation capabilities:
- **[CorrelationIdProvider](CorrelationId/CorrelationIdProvider.md)** - Correlation ID generation and management
- **[CorrelationIdSupportHelper](CorrelationId/CorrelationIdSupportHelper.md)** - Helper utilities for correlation
- **[ICorrelationIdSupport](CorrelationId/ICorrelationIdSupport.md)** - Interface for correlation support
- **[README](CorrelationId/README.md)** - Correlation concepts and implementation

### Enumerations
The `Enums/` folder defines common enumeration types:
- **[AuthenticationType](Enums/AuthenticationType.md)** - Authentication method enumeration
- **[CastType](Enums/CastType.md)** - Type casting options
- **[DataType](Enums/DataType.md)** - Data type classifications
- **[RecoveryStorage](Enums/RecoveryStorage.md)** - Storage recovery options
- **[README](Enums/README.md)** - Enumeration usage guidelines

### Helper Utilities
The `Helpers/` folder contains various utility classes:
- **[CollectionHelper](Helpers/CollectionHelper.md)** - Collection manipulation utilities
- **[ConnectionStringHelper](Helpers/ConnectionStringHelper.md)** - Database connection string utilities
- **[DateTimeHelper](Helpers/DateTimeHelper.md)** - Date and time manipulation utilities
- **[EnvironmentHelper](Helpers/EnvironmentHelper.md)** - Environment variable and system utilities
- **[ExceptionHelper](Helpers/ExceptionHelper.md)** - Exception handling and analysis utilities
- **[GuardClauseHelper](Helpers/GuardClauseHelper.md)** - Parameter validation and guard clauses
- **[JsonHelper](Helpers/JsonHelper.md)** - JSON serialization and manipulation
- **[JwtIdentityHelper](Helpers/JwtIdentityHelper.md)** - JWT token handling utilities
- **[MessagePackHelper](Helpers/MessagePackHelper.md)** - MessagePack serialization utilities
- **[NetJsonHelper](Helpers/NetJsonHelper.md)** - NetJSON serialization utilities
- **[NJsonHelper](Helpers/NJsonHelper.md)** - Newtonsoft.Json utilities
- **[ObjectHelper](Helpers/ObjectHelper.md)** - Object manipulation and reflection utilities
- **[ProtobufHelper](Helpers/ProtobufHelper.md)** - Protocol Buffers serialization utilities
- **[Size](Helpers/Size.md)** - Size and measurement utilities
- **[StreamHelper](Helpers/StreamHelper.md)** - Stream processing utilities
- **[StringHelper](Helpers/StringHelper.md)** - String manipulation and processing utilities
- **[YamlHelper](Helpers/YamlHelper.md)** - YAML serialization and processing utilities
- **[README](Helpers/README.md)** - Helper utilities overview

### Identity Management
The `Identity/` folder provides authentication and authorization components:
- **[BasicUserConfiguration](Identity/BasicUserConfiguration.md)** - Basic user configuration model
- **[JwtConfiguration](Identity/JwtConfiguration.md)** - JWT authentication configuration
- **[README](Identity/README.md)** - Identity management overview

### Object Models
The `Objects/` folder contains base object models and patterns:
- **[CompressedObject](Objects/CompressedObject.md)** - Object with compression support
- **[DisposableObject](Objects/DisposableObject.md)** - Base class for disposable objects
- **[EquatableObject](Objects/EquatableObject.md)** - Base class for equatable objects
- **[ImmutableObject](Objects/ImmutableObject.md)** - Base class for immutable objects
- **[NotifiableObject](Objects/NotifiableObject.md)** - Base class for property change notification
- **[README](Objects/README.md)** - Object model patterns and usage

### Serialization
The `Serializations/` folder provides serialization abstractions and implementations:
- **[KafkaSerializerType](Serializations/KafkaSerializerType.md)** - Kafka serialization type enumeration
- **[SerializerType](Serializations/SerializerType.md)** - General serializer type enumeration
- **[README](Serializations/README.md)** - Serialization overview and patterns

#### JSON Serialization
- **[JsonConverter](Serializations/Json/JsonConverter.md)** - Custom JSON converter implementations

#### YAML Serialization
- **[YamlNodeDeserializerAttribute](Serializations/Yaml/YamlNodeDeserializerAttribute.md)** - YAML node deserializer attribute
- **[YamlSerializerSettings](Serializations/Yaml/YamlSerializerSettings.md)** - YAML serialization configuration
- **[YamlTypeConverter](Serializations/Yaml/YamlTypeConverter.md)** - YAML type conversion utilities
- **[YamlTypeConverterAttribute](Serializations/Yaml/YamlTypeConverterAttribute.md)** - YAML type converter attribute
- **[README](Serializations/Yaml/README.md)** - YAML serialization overview

## Getting Started

These building blocks are designed to work together to provide a comprehensive foundation for building robust applications. Each component is documented with:

- **Purpose and use cases**
- **API reference and examples**
- **Integration guidelines**
- **Best practices and patterns**

### Common Usage Patterns

1. **Start with core components** like `ServiceConfiguration` and `Telemetry` for basic application infrastructure
2. **Add specialized functionality** as needed from the various folders (Collections, Helpers, etc.)
3. **Use change tracking** for data-centric applications that need audit trails
4. **Implement correlation IDs** for distributed systems and request tracing
5. **Leverage helper utilities** to reduce boilerplate code and improve consistency

### Integration Notes

- All components follow consistent naming conventions and patterns
- Most components are designed to work with dependency injection containers
- Thread-safety is clearly documented for each component
- Performance characteristics are documented where relevant

For specific implementation details and examples, refer to the individual component documentation files.