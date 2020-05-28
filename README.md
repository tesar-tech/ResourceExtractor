# Resource Extractor

Keep your strings of primary language in xaml. Extract them by running this script.

![](media/2018-04-10-17-36-48.png)

## How to use it

1. Edit `resourceExtractor.fsx` to match folder of your project and path to `Resource.resw` file. Create folder `Strings\en-US` if not exits.

    ``` fsharp
    let folderToDeepSearchForFiles = __SOURCE_DIRECTORY__  + @"\ResourceExtractorSampleApp\ResourceExtractorSampleApp"
    let finalResourceFilePath =  folderToDeepSearchForFiles + @"\Strings\en-US\Resources.resw"
    ```

2. Respect some necessary conventions:
   - Place properties, that you want to extract, after the `x:Uid` property. Last character of the `x:Uid` matches number of subsequent properties to be extracted. If last character is not a number, only one subsequent property will be taken.

   ![](media/2018-04-10-16-41-34.png)

   - For extracting resources from .cs files use `ReousrceLoader`. "TextFromCSharpCode" string will be taken as resource name and comment as default value: 

    ``` csharp
    ResourceLoader.GetForCurrentView().GetString("TextFromCSharpCode");//This default text is placed in comment in MainPage.xaml.cs`
    ```
3. Run F# script. Note that existing `en-US\Resource.resw` file will be overwritten.For complete example, see sample UWP app.

## The flow with Multilingual app toolkit

Start with instalation of [Multilingual app toolkit extension](https://marketplace.visualstudio.com/items?itemName=MultilingualAppToolkit.MultilingualAppToolkit-18308) and [Multilingual app toolkit Editor](https://developer.microsoft.com/en-us/windows/downloads/multilingual-app-toolkit/)

1. Write your xaml with `x:Uid` (follow instructions above)
2. Run `resourceExtractor.fsx` script. This will generate `en-US\Resource.resw`.
3. Build your app. This will generate `xlf` files under `MultilingualResources` folder.
4. Use Multilingual app toolkit Editor to translate your strings.
5. Build app again. It will generate translated `Resource.resw` files.

## Limitations

- There are only two possible outputs:
  - Rewritten `en-US\Resource.resw` file.
  - Error in terminal.
- Script does not create folder structure for you (`Strings\en-US\`).
- `en-US\Resource.resw` created by script is not automatically included in UWP project (you need to include it in project just once)

## Contributing

If you have any idea how to make script or sample app better, feel free to submit issue or pull-request.
