# Development Workflow & Conventions

## Project Structure Context
```
L:\workspace\ZeitLabs\fj-leb128\src\
├── fj-leb128.sln                 # Main solution file
├── FJ.Leb128\                    # Main library project
│   ├── FJ.Leb128.csproj         # .NET 8 library, MIT license
│   ├── Encoding\                # Encoding implementations  
│   ├── Decoding\                # Decoding implementations
│   └── *.cs                     # Core types (Size, *Info)
└── FJ.Leb128.Tests\             # Test project
    ├── FJ.Leb128.Tests.csproj   # xUnit + FluentAssertions
    ├── Encoding\                # Encoding-specific tests
    ├── Decoding\                # Decoding-specific tests
    └── *Tests.cs                # Core test files
```

## Build & Test Commands
```bash
# From src/ directory:
dotnet build                      # Build solution
dotnet test                       # Run all 838 tests
dotnet test --verbosity normal    # Detailed test output
dotnet pack                       # Create NuGet package (if configured)
```

## Code Quality Standards
```csharp
// .NET 8 features used:
// - Nullable reference types: enabled
// - Implicit usings: enabled  
// - Primary constructors: used in SignedInfo
// - Collection expressions: used in test data arrays

// Code analysis:
// - CA1707 suppressed for test naming (underscores allowed)
// - All other CA rules enforced
// - XML documentation required for public APIs
```

## Git Workflow Context  
```bash
# Current state (from overview):
Current branch: main
Main branch: main
Status: (clean)

Recent commits:
d96db7f Initial (#2)
9d6c470 Move project to its own repo (#1) 
fb74bf9 Initial commit
```

## Coding Conventions Applied
```csharp
// Naming:
// - Classes: PascalCase with descriptive names
// - Methods: PascalCase verbs (Encode, TryDecode)
// - Parameters: camelCase with clear intent
// - Constants: PascalCase or UPPER_CASE for emphasis

// Formatting:
// - Braces: Opening brace on same line for methods/classes
// - Spaces: Around operators, after commas
// - Line length: No strict limit, readability prioritized
// - Indentation: 4 spaces (consistent across codebase)
```

## File Organization Patterns
```csharp
// Each file contains single primary class
// Related functionality grouped in same namespace folder:
// - FJ.Leb128.Encoding.UnsignedLeb128Encoding
// - FJ.Leb128.Encoding.SignedLeb128Encoding
// - FJ.Leb128.Decoding.UnsignedLeb128Decoding  
// - FJ.Leb128.Decoding.SignedLeb128Decoding

// Test files mirror source structure:
// - Encoding tests in Encoding/ subdirectory
// - Decoding tests in Decoding/ subdirectory
// - Integration tests in root test directory
```

## Documentation Standards
```xml
<!-- All public APIs have XML documentation -->
/// <summary>
/// Encodes a UInt128 value to a byte array using LEB128 encoding.
/// </summary>
/// <param name="i">The UInt128 value to encode.</param>
/// <param name="byteCount">The number of bytes used in the encoding.</param>
/// <returns>A byte array containing the LEB128 encoded value.</returns>
public static byte[] Encode(UInt128 i, out int byteCount) { ... }
```

## Performance Development Guidelines
```csharp
// When adding new APIs:
// 1. Span-based version first (modern, zero-allocation)
// 2. Traditional byte[] version for compatibility
// 3. Stream version if I/O scenarios needed
// 4. Try-pattern version for safe parsing

// Example progression:
bool TryEncode(value, destination, out bytesWritten)  // Primary
ReadOnlySpan<byte> Encode(value, buffer)              // Convenience
byte[] Encode(value, out byteCount)                   // Compatibility
```

## Test Development Workflow
```csharp
// Test-driven approach:
// 1. Write failing test with expected behavior
// 2. Implement minimal code to pass test
// 3. Refactor for performance/clarity
// 4. Add edge case tests
// 5. Validate with property-based testing (random inputs)

// Test data reuse strategy:
// - Create MemberData methods for complex test cases
// - Reuse data across multiple test classes
// - Focus on boundary conditions and edge cases
```

## Version Control Practices  
```bash
# Commit message format (inferred from history):
# - Descriptive title line
# - Reference issue numbers (#1, #2)  
# - Focus on user-facing changes

# Branch strategy:
# - main branch for stable code
# - Feature branches for new development (inferred)
# - PR-based workflow with reviews
```

## Package Management Configuration
```xml
<!-- FJ.Leb128.csproj includes package metadata -->
<PropertyGroup>
    <Title>LEB128 Encoder/Decoder</Title>
    <Authors>FJ Zeit</Authors>
    <Copyright>Copyright 2023-2024 FJ Zeit</Copyright>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageProjectUrl>https://github.com/fjzeit/fj-leb128</PackageProjectUrl>
    <Description>A .NET library for encoding and decoding integers using the LEB128...</Description>
    <RepositoryUrl>https://github.com/fjzeit/fj-leb128</RepositoryUrl>
</PropertyGroup>
```

## Future Development Considerations
```csharp
// API extension points:
// 1. Additional integer sizes (Int16, UInt16 direct support)
// 2. Async stream processing (IAsyncEnumerable)
// 3. Bulk operations (encode/decode arrays)
// 4. Unsafe optimizations for maximum performance
// 5. SIMD vectorization for parallel processing

// Breaking change management:
// - Maintain backward compatibility for existing APIs
// - Use [Obsolete] for deprecated methods
// - Version bumps for breaking changes
// - Clear migration guides in documentation
```

## IDE Configuration Context
```json
// .editorconfig likely present (referenced in csproj analysis)
// - Consistent formatting across team
// - Code analysis rule configuration
// - File encoding standards (UTF-8)
// - Line ending normalization
```

## Debugging & Troubleshooting
```csharp
// Common issues and solutions:
// 1. Span compilation errors: Check FluentAssertions usage
// 2. Test failures: Validate test data consistency
// 3. Performance issues: Profile with span vs allocation APIs
// 4. Encoding errors: Verify bit manipulation logic
// 5. Overflow issues: Check boundary conditions and max values
```