const { src, dest, series } = require('gulp');
const fs = require('fs');
const path = require('path');

// Configuration
const CONFIG = {
  // Source C# models directory
  csharpModelsPath: path.join(__dirname, '..', 'Domain', 'Models'),
  // Output TypeScript models directory
  typescriptModelsPath: path.join(__dirname, 'src', 'app', 'models', 'generated'),
  // File encoding
  encoding: 'utf8'
};

// Type mappings from C# to TypeScript
const TYPE_MAPPINGS = {
  'int': 'number',
  'long': 'number',
  'float': 'number',
  'double': 'number',
  'decimal': 'number',
  'bool': 'boolean',
  'string': 'string',
  'DateTime': 'string', // ISO date strings in JSON
  'Guid': 'string',
  'List<': 'Array<', // Will be handled specially
  'ICollection<': 'Array<',
  'IEnumerable<': 'Array<'
};

/**
 * Converts C# type to TypeScript type
 */
function convertType(csharpType) {
  // Handle nullable types
  let isNullable = csharpType.includes('?');
  let baseType = csharpType.replace('?', '').trim();
  
  // Handle generic types
  if (baseType.includes('List<') || baseType.includes('ICollection<') || baseType.includes('IEnumerable<')) {
    const genericMatch = baseType.match(/<(.+)>/);
    if (genericMatch) {
      const innerType = convertType(genericMatch[1]);
      return isNullable ? `${innerType}[] | null` : `${innerType}[]`;
    }
  }
  
  // Direct mapping
  const tsType = TYPE_MAPPINGS[baseType] || baseType;
  return isNullable ? `${tsType} | null` : tsType;
}

/**
 * Parses a C# property line
 */
function parseProperty(line) {
  // Match: public Type PropertyName { get; set; }
  const match = line.match(/public\s+(\S+\??)\s+(\w+)\s*{\s*get;\s*set;\s*}/);
  if (!match) return null;
  
  const [, type, name] = match;
  const tsType = convertType(type);
  const propertyName = name.charAt(0).toLowerCase() + name.slice(1); // camelCase
  
  return { name: propertyName, type: tsType };
}

/**
 * Parses C# XML comments
 */
function parseComment(lines, index) {
  let comment = '';
  let i = index - 1;
  
  while (i >= 0 && lines[i].trim().startsWith('///')) {
    const line = lines[i].trim();
    const summaryMatch = line.match(/\/\/\/\s*<summary>\s*(.+)\s*<\/summary>/);
    const commentMatch = line.match(/\/\/\/\s+(.+)/);
    
    if (summaryMatch) {
      comment = summaryMatch[1].trim();
      break;
    } else if (commentMatch && !line.includes('<summary>') && !line.includes('</summary>')) {
      comment = commentMatch[1].trim() + ' ' + comment;
    }
    i--;
  }
  
  return comment.trim();
}

/**
 * Parses a C# enum
 */
function parseEnum(content, className) {
  const lines = content.split('\n');
  const enumValues = [];
  let inEnum = false;
  
  for (let i = 0; i < lines.length; i++) {
    const line = lines[i].trim();
    
    if (line.includes(`enum ${className}`)) {
      inEnum = true;
      continue;
    }
    
    if (inEnum) {
      if (line === '}') break;
      
      // Match enum value with optional number assignment
      const match = line.match(/(\w+)\s*=\s*(\d+)/);
      if (match) {
        const comment = parseComment(lines, i);
        enumValues.push({
          name: match[1],
          value: match[1], // Use string values for enums
          comment
        });
      }
    }
  }
  
  return enumValues;
}

/**
 * Generates TypeScript interface from C# class
 */
function generateInterface(content, className) {
  const lines = content.split('\n');
  const properties = [];
  let comment = '';
  
  for (let i = 0; i < lines.length; i++) {
    const line = lines[i].trim();
    
    if (line.includes(`class ${className}`)) {
      comment = parseComment(lines, i);
      continue;
    }
    
    if (line.startsWith('public') && line.includes('{ get; set; }')) {
      const prop = parseProperty(line);
      if (prop) {
        const propComment = parseComment(lines, i);
        properties.push({ ...prop, comment: propComment });
      }
    }
  }
  
  // Generate TypeScript
  let tsContent = `/**\n * ${comment || `Generated from ${className}.cs`}\n * Auto-generated - do not modify manually\n */\n`;
  tsContent += `export interface ${className} {\n`;
  
  properties.forEach(prop => {
    if (prop.comment) {
      tsContent += `  /** ${prop.comment} */\n`;
    }
    tsContent += `  ${prop.name}: ${prop.type};\n`;
  });
  
  tsContent += '}\n';
  
  return tsContent;
}

/**
 * Generates TypeScript enum from C# enum
 */
function generateEnum(content, className) {
  const enumValues = parseEnum(content, className);
  if (enumValues.length === 0) return null;
  
  let tsContent = `/**\n * Generated from ${className}.cs\n * Auto-generated - do not modify manually\n */\n`;
  tsContent += `export enum ${className} {\n`;
  
  enumValues.forEach((value, index) => {
    if (value.comment) {
      tsContent += `  /** ${value.comment} */\n`;
    }
    tsContent += `  ${value.name} = '${value.value}'`;
    if (index < enumValues.length - 1) {
      tsContent += ',\n';
    } else {
      tsContent += '\n';
    }
  });
  
  tsContent += '}\n';
  
  return tsContent;
}

/**
 * Main task to generate TypeScript models from C# models
 */
function generateModels(done) {
  console.log('Starting model generation...');
  
  // Ensure output directory exists
  if (!fs.existsSync(CONFIG.typescriptModelsPath)) {
    fs.mkdirSync(CONFIG.typescriptModelsPath, { recursive: true });
  }
  
  // Read all C# model files
  const files = fs.readdirSync(CONFIG.csharpModelsPath)
    .filter(file => file.endsWith('.cs'));
  
  console.log(`Found ${files.length} C# model files`);
  
  files.forEach(file => {
    const filePath = path.join(CONFIG.csharpModelsPath, file);
    const content = fs.readFileSync(filePath, CONFIG.encoding);
    const className = path.basename(file, '.cs');
    
    let tsContent = '';
    
    // Check if it's an enum or class
    if (content.includes(`enum ${className}`)) {
      console.log(`Generating enum: ${className}`);
      tsContent = generateEnum(content, className);
    } else if (content.includes(`class ${className}`)) {
      console.log(`Generating interface: ${className}`);
      tsContent = generateInterface(content, className);
    }
    
    if (tsContent) {
      const outputFile = path.join(CONFIG.typescriptModelsPath, `${className.toLowerCase()}.model.ts`);
      fs.writeFileSync(outputFile, tsContent, CONFIG.encoding);
      console.log(`✓ Generated: ${outputFile}`);
    }
  });
  
  // Generate index file
  generateIndexFile(files);
  
  console.log('Model generation complete!');
  done();
}

/**
 * Generates an index.ts file that exports all models
 */
function generateIndexFile(files) {
  let indexContent = '/**\n * Auto-generated index file\n * Exports all generated models\n */\n\n';
  
  files.forEach(file => {
    const className = path.basename(file, '.cs');
    indexContent += `export * from './${className.toLowerCase()}.model';\n`;
  });
  
  const indexPath = path.join(CONFIG.typescriptModelsPath, 'index.ts');
  fs.writeFileSync(indexPath, indexContent, CONFIG.encoding);
  console.log(`✓ Generated index file: ${indexPath}`);
}

// Export tasks
exports['generate-models'] = series(generateModels);
exports.default = series(generateModels);
