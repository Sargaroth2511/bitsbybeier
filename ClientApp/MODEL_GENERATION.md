# Automatic Model Generation

This project uses Gulp to automatically generate TypeScript models from C# backend models.

## Overview

The `gulpfile.js` contains a task that:
1. Reads C# model files from `../Domain/Models/`
2. Parses class definitions, enums, properties, and XML comments
3. Generates corresponding TypeScript interfaces and enums
4. Outputs files to `src/app/models/generated/`

## Usage

Generate models from C# definitions:

```bash
npm run generate-models
```

This will:
- Create TypeScript interfaces for each C# class
- Create TypeScript enums for each C# enum
- Generate an `index.ts` file that exports all models
- Preserve XML documentation comments

## Generated Files

All generated files are placed in `src/app/models/generated/` and include:
- `*.model.ts` - Individual model files
- `index.ts` - Barrel export file

**Important**: Do not manually edit generated files. They will be overwritten on the next generation run.

## Type Mappings

C# types are automatically converted to TypeScript:

| C# Type | TypeScript Type |
|---------|----------------|
| `int`, `long`, `float`, `double`, `decimal` | `number` |
| `bool` | `boolean` |
| `string` | `string` |
| `DateTime` | `string` (ISO date) |
| `Guid` | `string` |
| `List<T>`, `ICollection<T>`, `IEnumerable<T>` | `T[]` |
| `T?` (nullable) | `T \| null` |

## Features

- ✅ Parses C# classes and enums
- ✅ Converts property types to TypeScript equivalents
- ✅ Handles nullable types (`?`)
- ✅ Handles generic collections (`List<T>`)
- ✅ Preserves XML documentation comments
- ✅ Converts PascalCase to camelCase for properties
- ✅ Generates barrel export file

## Example

**C# Model** (`Domain/Models/UserRole.cs`):
```csharp
/// <summary>
/// Defines user roles in the system.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Standard user with basic permissions.
    /// </summary>
    User = 0,
    
    /// <summary>
    /// Administrator with full system access.
    /// </summary>
    Admin = 1
}
```

**Generated TypeScript** (`src/app/models/generated/userrole.model.ts`):
```typescript
/**
 * Generated from UserRole.cs
 * Auto-generated - do not modify manually
 */
export enum UserRole {
  /** Standard user with basic permissions. */
  User = 'User',
  /** Administrator with full system access. */
  Admin = 'Admin'
}
```

## Configuration

To modify the generator behavior, edit `gulpfile.js`:

```javascript
const CONFIG = {
  // Source C# models directory
  csharpModelsPath: path.join(__dirname, '..', 'Domain', 'Models'),
  // Output TypeScript models directory
  typescriptModelsPath: path.join(__dirname, 'src', 'app', 'models', 'generated'),
  // File encoding
  encoding: 'utf8'
};
```

## Limitations

Current limitations:
- Does not parse C# records (like `HealthStatus`)
- Does not handle inheritance
- Does not handle interfaces
- Does not handle generic classes (only generic properties)

These can be added as needed in future iterations.

## Integration with Build

You can add model generation to your build process by modifying `package.json`:

```json
"scripts": {
  "prebuild": "npm run generate-models",
  "build": "ng build"
}
```

This will automatically regenerate models before each build.
