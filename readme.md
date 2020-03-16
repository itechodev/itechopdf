# Itechopdf 
Itechopdf  is a dotnet corelibrary around the wkhtmltopdf utility to produce PDF files from HTML. You can create multiple pages, cover pages and configurable header and footers using a simple API.

## Features
 * Create multiple pages with different configurations. This allows you to create cover pages etc.
 * Full support for html-based headers and footers.

## Installation
Package available on nuget.
```
dotnet add package itechopdf 
```
```
PM> Install-Package itechopdf 
```

## Usage
Itechopdf is built around three entities: **PdfRenderer**, **PdfDocument** and **PdfSource**.

**PdfSource** specify where the html source comes from. Either from a file or from a html string. Use the static constructors to instantiate the objects:
```csharp
var fileSource = PdfSource.FromFile("content.html");
var HtmlSource = PdfSource.FromHtml("<p>Content</p>");
```
**PdfDocument**. The PdfDocument refers to a page or pages created from a PdfSource with configuration like page orientation, sizes, margins, headers and footers etc. 
```csharp
var doc = new PdfDocument(PdfSource);
doc.Configure(print => {
    // Set page(s) configurations here
    print.Orientation = Orientation.Portrait;
    print.PrintBackground = true;
    print.ColorMode = ColorMode.Color;
    print.Margins.Set(0, 0, 0, 0, Unit.Millimeters);
});

// Set headers and footers using another PdfSource, height and line spacing.
doc.SetHeader(PdfSource.FromFile("header.html"), 25, 5);
doc.SetFooter(PdfSource.FromFile("footer.html"), 25, 5);
```

**PdfRenderer*.* The render document take multiple documents and merge into one.
```csharp
var renderer = new PdfRenderer();
renderer.Add(doc);
var bytes = renderer.RenderToBytes();
File.WriteAllBytes("output.pdf", bytes);
```
**Variables in headers and footers**
To use a variable you have to wrap it in the <var> tag. See why we favour the var tag syntax above moustache syntax [here](https://github.com/itechodev/itechopdf/issues/11).
    
```html
    <var text-align="right" text="[page] of [pages]" digits="2">10 of 10</var>
```


Variable replacement is done through what we call PDF stamping. This is a post PDF creation process and therefore a bit more complicated that regular variable replacement.



This was by far the most changelling concept to implement.


## HTML Caveats and limitations
* Flex not supported
* SVG images should have explicit width and height
* Using a retina display on OSX will render very small. This is because wkhtmltox uses your screen dimensions to create the PDF. Current solution is to play around with the DPI.
* CSS transforms should be prefixed with -webkit-.
* Performance. The underlying Wkhtmltopdf does not support multihreading.

## Why another library?
I needed a solution to produce rich PDF documents from HTML. By rich I mean: cover pages, multiple pages with potential different sizes and orientations, customizable headers and footers etc. None of the open source libraries could do that easily. There are commercial solutions but the pricetag scared me off, so I wrote my own.

## How does it work
Really simple actually.
1. HTML is inspected and changed to ensure correct paths, fix doctypes and add resource files including javascript and css. This is done using HtmlAgilityPack
2. Covert HTML to PDF using wkhtmltopdf
3. Merge all PDF's using PDFsharp

