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

| Attribute        | Options           | Meaning  | Default |
| -----------------|-------------------| ---------|---------|
| text-align | left, center, right | Align the replacement text relative to the bounding box | left |
| text  | variables enclosed with [] with any text | The actual text to replace | |
| digits | numberic | The original bounding box is formed by the maximum number of digits the variables can hold. In the example above [page] and [pages] will be replaced by 55 (two digits) to form the maximum bounding box. | 2 |
| innerText | Any string | Only for design purposes. Design your template to what the replacement might look like | |

| Variable Name | Desc |
|---------------|------|
| page | The current page in the PDF |
| pages | The total number of pages in the PDF|
| documentpage | The current page of the PdfDocument. Remember that a PDF may consists of multiple documents |
| documentpages | The total number of pages in the document |

Variable replacement is done through what we call PDF stamping. This is a post PDF creation process and therefore a bit more complicated that regular variable replacement.

This was by far the most changelling concept to implement.

## HTML Caveats and limitations
* Flex not supported
* SVG images should have explicit width and height
* Html tables will always have a border/outline. Use CSS-based table to remove this.
* CSS transforms should be prefixed with -webkit-.
* Using a retina display on OSX will render very small. This is because wkhtmltox uses your screen dimensions to create the PDF. Current solution is to play around with the DPI.
* Performance. The underlying Wkhtmltopdf does not support multihreading.

## Why another library?
I needed a solution to produce rich PDF documents from HTML. By rich I mean: cover pages, multiple pages with potential different sizes and orientations, customizable headers and footers etc. None of the open source libraries could do that easily. There are commercial solutions but the pricetag scared me off, so I wrote my own.

## How does it work
Really simple actually. For each document:
1. HTML is inspected and changed to ensure correct paths, fix doctypes and add resource files including javascript and css. Variables is converted to anchors with dummy text with a specific link making it identifiable in the PDF. This is done using HtmlAgilityPack
2. HTML is then converted to PDF using wkhtmltopdf.
3. These PDF's is then inspected to extract the variables and their placement- and style properties.
4. All regions overlapping the (anchor or variable) is replace (stamping) by its new value.
5. All seperate PDF's is then merged into one. All PDF operations is done by PDFClown.

