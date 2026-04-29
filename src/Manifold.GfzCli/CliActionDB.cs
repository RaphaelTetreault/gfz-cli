namespace Manifold.GfzCli;

/// <summary>
///     DataBase of <see cref="CliAction"/>.
/// </summary>
public static class CliActionDB
{
    #region Program

    private static readonly CliAction ActionUsage = new()
    {
        Description = "Call to print out actions available and how to use them.",
        Action = GfzCli.PrintActionUsage,
        ActionID = CliActionID.usage,
        InputIO = CliActionIO.None,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.None,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    private static readonly CliAction ActionList = new()
    {
        Description = "List all possible actions with description.",
        Action = GfzCli.PrintActionList,
        ActionID = CliActionID.list,
        InputIO = CliActionIO.None,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.None,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    private static readonly CliAction ActionNone = new()
    {
        Description = "No action selected.",
        Action = GfzCli.PrintActionUsage,
        ActionID = CliActionID.none,
        InputIO = CliActionIO.None,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.None,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly CliAction ArcPack = new()
    {
        Description = "Archive a directory into a .arc file.",
        Action = CliActions.ArcPack,
        ActionID = CliActionID.arc_pack,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Directory,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.Anything,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly CliAction ArcUnpack = new()
    {
        Description = "Unpack one or more .arc achives into directories of their contents.",
        Action = CliActions.ArcUnpack,
        ActionID = CliActionID.arc_unpack,
        InputIO = CliActionIO.Directory,
        OutputIO = CliActionIO.Directory,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.ARC,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    #endregion

    #region Asset

    public static readonly CliAction AssetGenerateLibrary = new()
    {
        Description = "Create a text-reference-linked GMA and TPL library.",
        Action = CliActions.AssetGenerateLibrary,
        ActionID = CliActionID.asset_generate_library,
        InputIO = CliActionIO.Directory,
        OutputIO = CliActionIO.Directory,
        IsOutputOptional = false,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [
        CliArgumentDB.ResamplerType,
            ],
    };

    public static readonly CliAction ActionAssetImageToGxtex = new()
    {
        Description = "Convert image to a raw GameCube GX texture.",
        Action = CliActionsAsset.ImageToGxTexture,
        ActionID = CliActionID.asset_image_to_gxtex,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = false,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [
            CliArgumentDB.MipmapFiles,
            CliArgumentDB.MipmapCount,
            CliArgumentDB.MipmapMode,
            CliArgumentDB.TextureFormat,
            // Resize
            CliArgumentDB.Width, // Size.X
            CliArgumentDB.Height, // Size.Y
            CliArgumentDB.Compand,
            CliArgumentDB.PadColor,
            CliArgumentDB.Position,
            CliArgumentDB.PremultiplyAlpha,
            CliArgumentDB.ResamplerType,
            CliArgumentDB.ResizeMode, // Mode
            ],
    };

    public static readonly CliAction ActionAssetCustomMipmapGxtex = new()
    {
        Description = "Convert images (main texture and mipmaps) to a raw GameCube GX texture.",
        Action = CliActionsAsset.ImagesToCustomMipmapGxtex,
        ActionID = CliActionID.asset_custom_mipmap_gxtex,
        InputIO = CliActionIO.File,
        OutputIO = CliActionIO.File,
        IsOutputOptional = false,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [
            CliArgumentDB.MipmapFiles,
            CliArgumentDB.MipmapCount,
            CliArgumentDB.MipmapMode,
            CliArgumentDB.TextureFormat,
            // Resize
            CliArgumentDB.Width, // Size.X
            CliArgumentDB.Height, // Size.Y
            CliArgumentDB.Compand,
            CliArgumentDB.PadColor,
            CliArgumentDB.Position,
            CliArgumentDB.PremultiplyAlpha,
            CliArgumentDB.ResamplerType,
            CliArgumentDB.ResizeMode, // Mode
            ],
    };

    public static readonly CliAction ActionAssetTplUnpack = new()
    {
        Description = "Unpack TPL files into TPLREFs.",
        Action = CliActionsAsset.TplUnpack,
        ActionID = CliActionID.asset_tpl_unpack,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.TPL,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [
            CliArgumentDB.DirFormat,
            ],
    };

    public static readonly CliAction ActionAssetTplrefPack = new()
    {
        Description = "Pack TPL file from TPLREF.",
        Action = CliActionsAsset.TplrefPack,
        ActionID = CliActionID.asset_tplref_pack,
        InputIO = CliActionIO.File,
        OutputIO = CliActionIO.File,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.TPLREF,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [
            CliArgumentDB.AssetLibraryRoot,
            ],
    };

    public static readonly CliAction ActionAssetGmarefPack = new()
    {
        Description = "Pack GMA file from GMAREF.",
        Action = CliActionsAsset.GmarefPack,
        ActionID = CliActionID.asset_gmaref_to_gma,
        InputIO = CliActionIO.File,
        OutputIO = CliActionIO.File,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.GMAREF,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [
            CliArgumentDB.AssetLibraryRoot,
            ],
    };

    #endregion

    #region Camera

    public static readonly CliAction CameraLivecamFromTSV = new()
    {
        Description = "Create livecam BIN file from livecam TSV spreadsheet.",
        Action = CliActions.CameraLivecamFromTSV,
        ActionID = CliActionID.cam_livecamstage_from_tsv,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.LivecamStage,
        FileProcessArgs = CliFileProcessArg.FOPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly CliAction CameraLivecamToTSV = new()
    {
        Description = "Create TSV from livecam binary.",
        Action = CliActions.CameraLivecamToTSV,
        ActionID = CliActionID.cam_livecamstage_to_tsv,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.LivecamStage,
        FileProcessArgs = CliFileProcessArg.FOPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    #endregion

    #region CarData

    public static readonly CliAction CarDataFromTSV = new()
    {
        Description = "Create a CarData.lz file from CarData TSV spreadsheet.",
        Action = CliActions.CarDataFromTsv,
        ActionID = CliActionID.cardata_from_tsv,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.FOPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly CliAction CarDataToTSV = new()
    {
        Description = "Create a TSV from CarData binary (compressed or uncompressed).",
        Action = CliActions.CarDataToTsv,
        ActionID = CliActionID.cardata_to_tsv,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.FOPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    #endregion

    #region

    public static readonly CliAction ColicourseEditFog = new()
    {
        Description = "Patch the fog parameters of scenes.",
        Action = CliActions.ColicourseEditFog,
        ActionID = CliActionID.colicourse_patch_fog,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.Colicourse,
        FileProcessArgs = CliFileProcessArg.FPS,
        RequiredArguments = [
            CliArgumentDB.Color,
            CliArgumentDB.ColorR,
            CliArgumentDB.ColorG,
            CliArgumentDB.ColorB,
            ],
        OptionalArguments = [
            CliArgumentDB.Backup,
            CliArgumentDB.FogInterpolationMode,
            CliArgumentDB.FogViewRangeNear,
            CliArgumentDB.FogViewRangeFar,
            ],
    };

    public static readonly CliAction ColicourseEditObjectRenderFlags = new()
    {
        Description = "Patch a scene object's render flags by name.",
        Action = CliActions.ColicourseEditObjectRenderFlags,
        ActionID = CliActionID.colicourse_patch_object_render_flags,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.Colicourse,
        FileProcessArgs = CliFileProcessArg.FPS,
        RequiredArguments = [
            CliArgumentDB.Name_ColiCourse,
            CliArgumentDB.Value_ColiCourse,
            ],
        OptionalArguments = [
            CliArgumentDB.Backup,
            CliArgumentDB.SetFlagsOff,
            ],
    };

    #endregion

    #region Emblem

    internal static CliAction EmblemGciToImage = new()
    {
        Description = "Extract images from GCI emblem save files.",
        Action = CliActions.EmblemGciToImage,
        ActionID = CliActionID.emblem_gci_to_image,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.GciEmblem,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [
        CliArgumentDB.ImageFormat,
            ],
    };

    internal static CliAction EmblemGciFromImage = new()
    {
        Description = "Create a GCI emblem save file from one image.",
        Action = CliActions.EmblemGciFromImage,
        ActionID = CliActionID.emblem_gci_from_image,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.ImagePNG,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [
            CliArgumentDB.ResamplerType,
            ],
        OptionalArguments = [
            CliArgumentDB.Compand,
            CliArgumentDB.ResizeMode,
            CliArgumentDB.PadColor,
            CliArgumentDB.Position,
            CliArgumentDB.PremultiplyAlpha,
            ],
    };

    internal static CliAction EmblemsBinToImages = new()
    {
        Description = "Extract images from emblem binary archives.",
        Action = CliActions.EmblemsBinToImages,
        ActionID = CliActionID.emblems_bin_to_images,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [
            CliArgumentDB.ImageFormat,
            ],
    };

    internal static CliAction EmblemsBinFromImages = new()
    {
        Description = "Compile an emblem binary archive from multiple images.",
        Action = CliActions.EmblemsBinFromImages,
        ActionID = CliActionID.emblems_bin_from_images,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.File,
        IsOutputOptional = false,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [
            CliArgumentDB.ResamplerType,
            ],
        OptionalArguments = [
            CliArgumentDB.Compand,
            CliArgumentDB.ResizeMode,
            CliArgumentDB.PadColor,
            CliArgumentDB.Position,
            CliArgumentDB.PremultiplyAlpha,
            ],
    };

    #endregion

    #region Encode Text

    public static readonly CliAction EncodeBytesToShiftJis = new()
    {
        Description = "Takes in hex-string of bytes and prints the Shift-JIS encoded version of the value.",
        Action = CliActions.EncodeBytesToShiftJis,
        ActionID = CliActionID.encode_bytes_to_shift_jis,
        InputIO = CliActionIO.None,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.None,
        RequiredArguments = [CliArgumentDB.Value_EncodeText],
        OptionalArguments = [],
    };

    public static readonly CliAction EncodeWindows1252ToShiftJis = new()
    {
        Description = "Takes in Windows code page 1252 string and prints the Shift-JIS encoded version of the value.",
        Action = CliActions.EncodeWindows1252ToShiftJis,
        ActionID = CliActionID.encode_windows_to_shift_jis,
        InputIO = CliActionIO.None,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.None,
        RequiredArguments = [CliArgumentDB.Value_EncodeText],
        OptionalArguments = [],
    };

    #endregion

    #region FMI

    public static readonly CliAction FmiFromPlainText = new()
    {
        Description = "Create a FMI-plaintext file from FMI binary file.",
        Action = CliActions.FmiFromPlainText,
        ActionID = CliActionID.fmi_from_plaintext,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.FmiPlaintext,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly CliAction FmiToPlainText = new()
    {
        Description = "Create a FMI binary file from FMI-plaintext.",
        Action = CliActions.FmiToPlainText,
        ActionID = CliActionID.fmi_to_plaintext,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.FMI,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    #endregion

    #region Ghost

    public static readonly CliAction GciExtractGhostFromGci = new()
    {
        Description = "Extract raw ghost data from GCI save file.",
        Action = CliActions.ExtractGhostFromGci,
        ActionID = CliActionID.gci_extract_ghost,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    #endregion

    #region GMA

    public static readonly CliAction GmaEditSubmeshRenderFlags = new()
    {
        Description = "Patch render flags on model submesh.",
        Action = CliActions.GmaEditSubmeshRenderFlags,
        ActionID = CliActionID.gma_patch_submesh_render_flags,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.GMA,
        FileProcessArgs = CliFileProcessArg.PS,
        RequiredArguments = [
            CliArgumentDB.Name_GMA,
            CliArgumentDB.Value_GMA],
        OptionalArguments = [
            CliArgumentDB.SetFlagsOff,
            ],
    };

    #endregion

    #region

    public static readonly CliAction IOGma = new()
    {
        Description = "Round-trip serialize GMA files.",
        Action = CliActions.InOutGMA,
        ActionID = CliActionID.io_gma,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.GMA,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly CliAction IOTpl = new()
    {
        Description = "Round-trip serialize TPL files.",
        Action = CliActions.InOutTPL,
        ActionID = CliActionID.io_tpl,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.TPL,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly CliAction IOScene = new()
    {
        Description = "Round-trip serialize COLI_COURSE (scene) files.",
        Action = CliActions.InOutScene,
        ActionID = CliActionID.io_scene,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.Colicourse,
        FileProcessArgs = CliFileProcessArg.OPRS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    // TODO: probably belongs in ActionsColiCourse
    public static readonly CliAction IOSceneNullComment = new()
    {
        Description = "Patch COLI_COURSE (scene) to null out auto-generate timestamp comment to help diff-ing.",
        Action = CliActions.IOSceneNullComment,
        ActionID = CliActionID.io_scene_null_comment,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.Colicourse,
        FileProcessArgs = CliFileProcessArg.PS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    #endregion

    #region ISO

    public static readonly CliAction IsoExtract = new()
    {
        Description = "Extract system data and files from GameCube ISO file.",
        Action = CliActions.IsoExtract,
        ActionID = CliActionID.iso_extract,
        InputIO = CliActionIO.File,
        OutputIO = CliActionIO.Directory,
        IsOutputOptional = false,
        FileProcessArgs = CliFileProcessArg.O,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly CliAction ActionIsoExtractFiles = new()
    {
        Description = "Extract files from GameCube ISO file.",
        Action = CliActions.IsoExtract,
        ActionID = CliActionID.iso_extract_files,
        InputIO = CliActionIO.File,
        OutputIO = CliActionIO.Directory,
        IsOutputOptional = false,
        FileProcessArgs = CliFileProcessArg.O,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly CliAction ActionIsoExtractSystem = new()
    {
        Description = "Extract system data from GameCube ISO file.",
        Action = CliActions.IsoExtract,
        ActionID = CliActionID.iso_extract_system,
        InputIO = CliActionIO.File,
        OutputIO = CliActionIO.Directory,
        IsOutputOptional = false,
        FileProcessArgs = CliFileProcessArg.O,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    #endregion

    #region Log

    public static readonly CliAction LogStageAll = new()
    {
        Description = "Create all possible analysis .TSVs of COLI_COURSE stage files.",
        Action = CliActions.LogStageAll,
        ActionID = CliActionID.log_stage_all,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Directory,
        IsOutputOptional = false,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.Colicourse,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly CliAction LogGmaAll = new()
    {
        Description = "Create all possible analysis .TSVs of GMA model files.",
        Action = CliActions.LogGmaAll,
        ActionID = CliActionID.log_gma_all,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Directory,
        IsOutputOptional = false,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.GMA,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    // TODO: for each one individually
    public static readonly CliAction LogStageTrackKeyablesAll = new()
    {
        Description = "Create a .tsv log of track keyables from COLI_COURSE stage files.",
        Action = CliActions.LogStageTrackKeyables,
        ActionID = CliActionID.log_stage_track_keyables,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Directory,
        IsOutputOptional = false,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.Colicourse,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    #endregion

    #region

    public static readonly CliAction LzCompress = new()
    {
        Description = "Compress files into an LZ file.",
        Action = CliActions.LzCompress,
        ActionID = CliActionID.lz_compress,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.FOPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly CliAction LzDecompress = new()
    {
        Description = "Decompress an LZ file.",
        Action = CliActions.LzDecompress,
        ActionID = CliActionID.lz_decompress,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    #endregion

    #region REL

    public static readonly CliAction FzMainRelPatchBgm = new()
    {
        Description = "Set the background music for a specific stage index.",
        Action = CliActions.FzMainRelPatchBgm,
        ActionID = CliActionID.fzrel_set_bgm,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.LineREL,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [
            CliArgumentDB.BgmIndex,
            CliArgumentDB.CourseIndex,
            ],
        OptionalArguments = [],
    };

    public static readonly CliAction FzMainRelPatchBgmFinalLap = new()
    {
        Description = "Set the final lap background music for a specific stage index.",
        Action = CliActions.FzMainRelPatchBgmFinalLap,
        ActionID = CliActionID.fzrel_set_bgmfl,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.LineREL,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [
            CliArgumentDB.BgmFinalLapIndex,
            CliArgumentDB.CourseIndex,
            ],
        OptionalArguments = [],
    };

    public static readonly CliAction FzMainRelPatchBgmBoth = new()
    {
        Description = "Set both default and final lap background music for a specific stage index.",
        Action = CliActions.FzMainRelPatchBgmBoth,
        ActionID = CliActionID.fzrel_set_bgm_bgmfl,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.LineREL,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [
            CliArgumentDB.BgmIndex,
            CliArgumentDB.BgmFinalLapIndex,
            CliArgumentDB.CourseIndex,
            ],
        OptionalArguments = [],
    };

    public static readonly CliAction FzMainRelPatchSetCourseDifficulty = new()
    {
        Description = "Set course difficulty star rating for a specific stage.",
        Action = CliActions.FzMainRelPatchSetCourseDifficulty,
        ActionID = CliActionID.fzrel_set_course_difficulty,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.LineREL,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [
            CliArgumentDB.CourseIndex,
            CliArgumentDB.Difficulty,
            ],
        OptionalArguments = [],
    };

    public static readonly CliAction FzMainRelPatchSetCourseName = new()
    {
        Description = "Set course name for a specific stage index.",
        Action = CliActions.PatchSetCourseName,
        ActionID = CliActionID.fzrel_set_course_name,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.LineREL,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [
            CliArgumentDB.CourseIndex,
            CliArgumentDB.Name_CourseName,
            ],
        OptionalArguments = [],
    };

    public static readonly CliAction FzMainRelPatchClearAllCourseNames = new()
    {
        Description = "Clear all names in course name table.",
        Action = CliActions.FzMainRelPatchClearAllCourseNames,
        ActionID = CliActionID.fzrel_clear_all_course_names,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.LineREL,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly CliAction FzMainRelPatchClearUnusedCourseNames = new()
    {
        Description = "Clear all unused course names in course name table.",
        Action = CliActions.FzMainRelPatchClearUnusedCourseNames,
        ActionID = CliActionID.fzrel_clear_unused_course_names,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.LineREL,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [CliArgumentDB.Name_CourseName],
        OptionalArguments = [],
    };

    public static readonly CliAction FzMainRelPatchSetCourseVenueIndex = new()
    {
        Description = "Set course venue for a specific stage index.",
        Action = CliActions.FzMainRelPatchSetCourseVenueIndex,
        ActionID = CliActionID.fzrel_set_course_venue,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.LineREL,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [
            CliArgumentDB.CourseIndex,
            CliArgumentDB.VenueIndex,
            ],
        OptionalArguments = [],
    };

    public static readonly CliAction FzMainRelPatchSetVenueName = new()
    {
        Description = "Set venue name for a specific venue index.",
        Action = CliActions.FzMainRelPatchSetVenueName,
        ActionID = CliActionID.fzrel_set_venue_name,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.LineREL,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [
            CliArgumentDB.VenueIndex,
            CliArgumentDB.Name_VenueName,
            ],
        OptionalArguments = [],
    };

    public static readonly CliAction FzMainRelPatchClearAllVenueNames = new()
    {
        Description = "Clear all names in venue name table.",
        Action = CliActions.FzMainRelPatchClearAllVenueNames,
        ActionID = CliActionID.fzrel_clear_all_venue_names,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.LineREL,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly CliAction FzMainRelPatchClearUnusedVenueNames = new()
    {
        Description = "Clear all unused course names in course name table.",
        Action = CliActions.FzMainRelPatchClearUnusedVenueNames,
        ActionID = CliActionID.fzrel_clear_unused_venue_names,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.LineREL,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [CliArgumentDB.Name_VenueName],
        OptionalArguments = [],
    };

    public static readonly CliAction FzMainRelPatchSetCarData = new()
    {
        Description = "Set \"graph console performance settings\" machine stats.",
        Action = CliActions.FzMainRelPatchSetCarData,
        ActionID = CliActionID.fzrel_set_cardata,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.LineREL,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [CliArgumentDB.Value_CarData],
        OptionalArguments = [],
    };

    public static readonly CliAction FzMainRelPatchMachineRating = new()
    {
        Description = "Set machine letter ratings (SABCDE).",
        Action = CliActions.FzMainRelPatchMachineRating,
        ActionID = CliActionID.fzrel_set_machine_rating,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.LineREL,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [
            CliArgumentDB.PilotNumber,
            CliArgumentDB.Value_MachineRating,
            ],
        OptionalArguments = [],
    };

    public static readonly CliAction FzMainRelPatchMaxSpeed = new()
    {
        Description = "Patch vehicle max speed.",
        Action = CliActions.FzMainRelPatchMaxSpeed,
        ActionID = CliActionID.fzrel_set_max_speed,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.LineREL,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [],
        OptionalArguments = [CliArgumentDB.Value_MaxSpeed],
    };

    public static readonly CliAction FzMainRelPatchSetCupCourse = new()
    {
        Description = "Set an individual stage reference in a cup.",
        Action = CliActions.FzMainRelPatchSetCupCourse,
        ActionID = CliActionID.fzrel_set_cup_course,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.LineREL,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [
            CliArgumentDB.Cup,           // cup to modify
            CliArgumentDB.CupCourseIndex, // stage in cup to modify 0-5 (count: 6)
            CliArgumentDB.CourseIndex,    // stage index to use
            ],
        OptionalArguments = [],
    };

    public static readonly CliAction FzMainRelDecryptLineREL = new()
    {
        Description = "Decrypt line__.bin to line__.rel file.", // TODO: rename output
        Action = CliActions.FzMainRelDecryptLineREL,
        ActionID = CliActionID.fzrel_decrypt,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.LineBIN,
        FileProcessArgs = CliFileProcessArg.OPRS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly CliAction FzMainRelEncryptLineREL = new()
    {
        Description = "Encrypt line__.rel to line__.bin file.", // TODO: rename output
        Action = CliActions.FzMainRelEncryptLineREL,
        ActionID = CliActionID.fzrel_encrypt,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        DefaultSearchPattern = CliArgumentText.SearchPatterns.LineREL,
        FileProcessArgs = CliFileProcessArg.OPRS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    #endregion

    public static readonly CliAction[] GfzCliActions =
    [
        // Program
        ActionNone,
        ActionList,
        ActionUsage,
        // ARC
        ArcPack,
        ArcUnpack,
        // ASSET LIBRARY
        AssetGenerateLibrary,
        ActionAssetCustomMipmapGxtex,
        ActionAssetGmarefPack,
        ActionAssetImageToGxtex,
        ActionAssetTplrefPack,
        ActionAssetTplUnpack,
        // CAMERA
        CameraLivecamFromTSV,
        CameraLivecamToTSV,
        // CARDATA
        CarDataFromTSV,
        CarDataToTSV,
        // COLICOURSE
        ColicourseEditFog,
        ColicourseEditObjectRenderFlags,
        // ENCODE TEXT
        EncodeBytesToShiftJis,
        EncodeWindows1252ToShiftJis,
        // EMBLEM
        EmblemGciFromImage,
        EmblemGciToImage,
        EmblemsBinFromImages,
        EmblemsBinToImages,
        // FMI
        FmiFromPlainText,
        FmiToPlainText,
        // GCI
        GciExtractGhostFromGci,
        // GMA
        GmaEditSubmeshRenderFlags,
        // ISO
        IsoExtract,
        ActionIsoExtractFiles,
        ActionIsoExtractSystem,
        // IO: IN-OUT TESTS
        IOGma,
        IOScene,
        IOSceneNullComment,
        IOTpl,
        // line__.rel
        FzMainRelDecryptLineREL,
        FzMainRelEncryptLineREL,
        FzMainRelPatchClearAllCourseNames,
        FzMainRelPatchClearAllVenueNames,
        FzMainRelPatchClearUnusedCourseNames,
        FzMainRelPatchClearUnusedVenueNames,
        FzMainRelPatchBgm,
        FzMainRelPatchBgmFinalLap,
        FzMainRelPatchBgmBoth,
        FzMainRelPatchSetCarData,
        FzMainRelPatchSetCourseName,
        FzMainRelPatchSetCupCourse,
        FzMainRelPatchMachineRating,
        FzMainRelPatchMaxSpeed,
        FzMainRelPatchSetCourseDifficulty,
        FzMainRelPatchSetCourseVenueIndex,
        FzMainRelPatchSetVenueName,
        // Log
        LogGmaAll,
        LogStageAll,
        LogStageTrackKeyablesAll,
        // LZ
        LzCompress,
        LzDecompress,
    ];


}
