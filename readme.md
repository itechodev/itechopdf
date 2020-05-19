# Itechopdf 
Itechopdf  is a dotnet corelibrary around the wkhtmltopdf utility to produce PDF files from HTML. You can create multiple pages, cover pages and configurable header and footers using a simple API.

## Features
 * Create multiple pages with different configurations. This allows you to create cover pages etc.
 * Full support for html-based dynamic headers and footers.

## Installation
Package available on nuget.
```
dotnet add package itechopdf 
```
```
PM> Install-Package itechopdf 
```
## Sample

```csharp
var renderer = new PdfRenderer();
// Create a landscape document with a 5mm margin
var doc = renderer.AddDocument(null, settings => 
{
    settings.PaperSize = PaperKind.A4;
    settings.Orientation = Orientation.Landscape;
    settings.Margins.Set(5, 5, 5, 5, Unit.Millimeters);
});

// Use a custom variable 'size' that will be replaced by the page number with a random color
doc.VariableResolver = (vars) => {
    var r = new Random();
    var color = $"rgb({r.Next(256)}, {r.Next(256)}, {r.Next(256)})";
    return new List<VariableReplace> 
    { 
        new VariableReplace("size", $"<span style=\"color:{color};\">{vars.Page}</span>")
    };
};

// Set header and footer for all pages in this document. 
doc.SetFooter(15, PdfSource.FromFile("default_footer.html"));
doc.SetHeader(30, PdfSource.FromFile("default_header.html"));

// Add a style for all pages and content
doc.AddCSS(PdfSource.FromFile("tailwind.min.css"));

doc.AddPage(PdfSource.FromFile("content.html"));
// Add another page with no header or footer
doc.AddPage(PdfSource.FromFile("content2.html", PdfSource.Empty(), PdfSource.Empty()));
// Add another page with custom header and footer
doc.AddPage(PdfSource.FromFile("content3.html", PdfSource.FromFile("custom_header.html"), PdfSource.FromFile("custom_footer.html)));

var bytes = renderer.RenderToBytes();
File.WriteAllBytes("output.pdf", bytes);
```

## Usage
Itechopdf is built around three entities: **PdfRenderer**, **PdfDocument** and **PdfSource**.

**PdfRenderer*.* Render multiple PdfDocuments and produce PDF.

**PdfSource** specify where the sources comes from. Either from a file or from a literal string. Use the static constructors to instantiate the objects:
```csharp
var fileSource = PdfSource.FromFile("content.html");
var htmlSource = PdfSource.FromHtml("<p>Content</p>");
```
**PdfDocument**. The PdfDocument is like a container holding multiple pages. All pages within the document shares the same page configuration like page orientation, page size, margins. Header and footers can be set on a document or per page or both.

**Variables in headers and footers**
To use a variable you have to wrap it in the <var> tag. See why we favour the var tag syntax above moustache syntax [here](https://github.com/itechodev/itechopdf/issues/11).
    
```html
    <var name="size">Design</var>
```
The are 6 variables per default. You can use the VariableResolver to add your own.
| Variable Name | Desc |
|---------------|------|
| page | The current page in the PDF |
| pages | The total number of pages in the PDF|
| document | The current page of the PdfDocument. Remember that a PDF may consists of multiple documents |
| documents | The total number of pages in the document |
| overflow | The current page overflow.  |
| overflows | The total number of overflows for the current page. |

## HTML Caveats and limitations
* Flex not supported
* SVG images should have explicit width and height
* Html tables will always have a border/outline. Use CSS-based table to remove this.
* CSS transforms should be prefixed with -webkit-.
* Using a retina display on OSX will render very small. This is because wkhtmltox uses your screen dimensions to create the PDF. Current solution is to play around with the DPI.
* Performance. The underlying Wkhtmltopdf does not support multihreading. 

## Why another library?
I needed a solution to produce rich PDF documents from HTML. By rich I mean: cover pages, multiple pages with potential different sizes and orientations, customizable headers and footers etc. None of the open source libraries could do that easily. And even if they did they took 6x longer than mine. 


