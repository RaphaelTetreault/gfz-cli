namespace Manifold.GfzCli;

/// <summary>
///     DataBase of <see cref="GfzCliAction"/>.
/// </summary>
public static class GfzCliActionDB
{
    #region Program

    private static readonly GfzCliAction ActionUsage = new()
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

    private static readonly GfzCliAction ActionList = new()
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

    private static readonly GfzCliAction ActionNone = new()
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

    public static readonly GfzCliAction ActionArcPack = new()
    {
        Description = "Archive a directory into a .arc file.",
        Action = CliActions.ArcPack,
        ActionID = CliActionID.arc_pack,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Directory,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionArcUnpack = new()
    {
        Description = "Unpack one or more .arc achives into directories of their contents.",
        Action = CliActions.ArcUnpack,
        ActionID = CliActionID.arc_unpack,
        InputIO = CliActionIO.Directory,
        OutputIO = CliActionIO.Directory,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    #endregion

    #region Asset

    public static readonly GfzCliAction ActionAssetGenerateLibrary = new()
    {
        Description = "Create a text-reference-linked GMA and TPL library.",
        Action = CliActions.GenerateLibrary,
        ActionID = CliActionID.asset_generate_library,
        InputIO = CliActionIO.Directory,
        OutputIO = CliActionIO.Directory,
        IsOutputOptional = false,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [
        GfzCliArgumentDB.ResamplerType,
            ],
    };

    public static readonly GfzCliAction ActionAssetImageToGxtex = new()
    {
        Description = "Convert image to a raw GameCube GX texture.",
        Action = ActionsAsset.ImageToGxTexture,
        ActionID = CliActionID.asset_image_to_gxtex,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = false,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [
            GfzCliArgumentDB.MipmapFiles,
            GfzCliArgumentDB.MipmapCount,
            GfzCliArgumentDB.MipmapMode,
            GfzCliArgumentDB.TextureFormat,
            // Resize
            GfzCliArgumentDB.Width, // Size.X
            GfzCliArgumentDB.Height, // Size.Y
            GfzCliArgumentDB.Compand,
            GfzCliArgumentDB.PadColor,
            GfzCliArgumentDB.Position,
            GfzCliArgumentDB.PremultiplyAlpha,
            GfzCliArgumentDB.ResamplerType,
            GfzCliArgumentDB.ResizeMode, // Mode
            ],
    };

    public static readonly GfzCliAction ActionAssetCustomMipmapGxtex = new()
    {
        Description = "Convert images (main texture and mipmaps) to a raw GameCube GX texture.",
        Action = ActionsAsset.ImagesToCustomMipmapGxtex,
        ActionID = CliActionID.asset_custom_mipmap_gxtex,
        InputIO = CliActionIO.File,
        OutputIO = CliActionIO.File,
        IsOutputOptional = false,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [
            GfzCliArgumentDB.MipmapFiles,
            GfzCliArgumentDB.MipmapCount,
            GfzCliArgumentDB.MipmapMode,
            GfzCliArgumentDB.TextureFormat,
            // Resize
            GfzCliArgumentDB.Width, // Size.X
            GfzCliArgumentDB.Height, // Size.Y
            GfzCliArgumentDB.Compand,
            GfzCliArgumentDB.PadColor,
            GfzCliArgumentDB.Position,
            GfzCliArgumentDB.PremultiplyAlpha,
            GfzCliArgumentDB.ResamplerType,
            GfzCliArgumentDB.ResizeMode, // Mode
            ],
    };

    public static readonly GfzCliAction ActionAssetTplUnpack = new()
    {
        Description = "Unpack TPL files into TPLREFs.",
        Action = ActionsAsset.TplUnpack,
        ActionID = CliActionID.asset_tpl_unpack,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [
            GfzCliArgumentDB.DirFormat,
            ],
    };

    public static readonly GfzCliAction ActionAssetTplrefPack = new()
    {
        Description = "Pack TPL file from TPLREF.",
        Action = ActionsAsset.TplrefPack,
        ActionID = CliActionID.asset_tplref_pack,
        InputIO = CliActionIO.File,
        OutputIO = CliActionIO.File,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [
            GfzCliArgumentDB.AssetLibraryRoot,
            ],
    };

    public static readonly GfzCliAction ActionAssetGmarefPack = new()
    {
        Description = "Pack GMA file from GMAREF.",
        Action = ActionsAsset.GmarefPack,
        ActionID = CliActionID.asset_gmaref_to_gma,
        InputIO = CliActionIO.File,
        OutputIO = CliActionIO.File,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [
            GfzCliArgumentDB.AssetLibraryRoot,
            ],
    };

    #endregion

    #region Camera

    public static readonly GfzCliAction ActionCameraLivecamFromTSV = new()
    {
        Description = "Create livecam BIN file from livecam TSV spreadsheet.",
        Action = CliActions.LivecamFromTsv,
        ActionID = CliActionID.cam_livecamstage_from_tsv,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.FOPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionCameraLivecamToTSV = new()
    {
        Description = "Create TSV from livecam binary.",
        Action = CliActions.LivecamToTsv,
        ActionID = CliActionID.cam_livecamstage_to_tsv,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.FOPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    #endregion

    #region CarData

    public static readonly GfzCliAction ActionCarDataFromTSV = new()
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

    public static readonly GfzCliAction ActionCarDataToTSV = new()
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

    public static readonly GfzCliAction ActionColicoursePatchFog = new()
    {
        Description = "Patch the fog parameters of scenes.",
        Action = CliActions.PatchFog,
        ActionID = CliActionID.colicourse_patch_fog,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.FPS,
        RequiredArguments = [
            GfzCliArgumentDB.Color,
            GfzCliArgumentDB.ColorR,
            GfzCliArgumentDB.ColorG,
            GfzCliArgumentDB.ColorB,
            ],
        OptionalArguments = [
            GfzCliArgumentDB.Backup,
            GfzCliArgumentDB.FogInterpolationMode,
            GfzCliArgumentDB.FogViewRangeNear,
            GfzCliArgumentDB.FogViewRangeFar,
            ],
    };

    public static readonly GfzCliAction ActionColicoursePatchObjectRenderFlags = new()
    {
        Description = "Patch a scene object's render flags by name.",
        Action = CliActions.PatchSceneObjectDynamicRenderFlags,
        ActionID = CliActionID.colicourse_patch_object_render_flags,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.FPS,
        RequiredArguments = [
            GfzCliArgumentDB.Name_ColiCourse,
            GfzCliArgumentDB.Value_ColiCourse,
            ],
        OptionalArguments = [
            GfzCliArgumentDB.Backup,
            GfzCliArgumentDB.SetFlagsOff,
            ],
    };

    #endregion

    #region Emblem

    internal static GfzCliAction ActionEmblemGciToImage = new()
    {
        Description = "Extract images from GCI emblem save files.",
        Action = CliActions.EmblemGciToImage,
        ActionID = CliActionID.emblem_gci_to_image,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [
        GfzCliArgumentDB.ImageFormat,
            ],
    };

    internal static GfzCliAction ActionEmblemGciFromImage = new()
    {
        Description = "Create a GCI emblem save file from one image.",
        Action = CliActions.EmblemGciFromImage,
        ActionID = CliActionID.emblem_gci_from_image,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [
            GfzCliArgumentDB.ResamplerType,
            ],
        OptionalArguments = [
            GfzCliArgumentDB.Compand,
            GfzCliArgumentDB.ResizeMode,
            GfzCliArgumentDB.PadColor,
            GfzCliArgumentDB.Position,
            GfzCliArgumentDB.PremultiplyAlpha,
            ],
    };

    internal static GfzCliAction ActionEmblemsBinToImages = new()
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
            GfzCliArgumentDB.ImageFormat,
            ],
    };

    internal static GfzCliAction ActionEmblemsBinFromImages = new()
    {
        Description = "Compile an emblem binary archive from multiple images.",
        Action = CliActions.EmblemsBinFromImages,
        ActionID = CliActionID.emblems_bin_from_images,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.File,
        IsOutputOptional = false,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [
            GfzCliArgumentDB.ResamplerType,
            ],
        OptionalArguments = [
            GfzCliArgumentDB.Compand,
            GfzCliArgumentDB.ResizeMode,
            GfzCliArgumentDB.PadColor,
            GfzCliArgumentDB.Position,
            GfzCliArgumentDB.PremultiplyAlpha,
            ],
    };

    #endregion

    #region Encode Text

    public static readonly GfzCliAction ActionEncodeBytesToShiftJis = new()
    {
        Description = "Takes in hex-string of bytes and prints the Shift-JIS encoded version of the value.",
        Action = CliActions.PrintBytesToShiftJis,
        ActionID = CliActionID.encode_bytes_to_shift_jis,
        InputIO = CliActionIO.None,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.None,
        RequiredArguments = [GfzCliArgumentDB.Value_EncodeText],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionEncodeWindows1252ToShiftJis = new()
    {
        Description = "Takes in Windows code page 1252 string and prints the Shift-JIS encoded version of the value.",
        Action = CliActions.PrintWindowsToShiftJis,
        ActionID = CliActionID.encode_windows_to_shift_jis,
        InputIO = CliActionIO.None,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.None,
        RequiredArguments = [GfzCliArgumentDB.Value_EncodeText],
        OptionalArguments = [],
    };

    #endregion

    #region FMI

    public static readonly GfzCliAction ActionFmiFromPlainText = new()
    {
        Description = "Create a FMI-plaintext file from FMI binary file.",
        Action = CliActions.FmiFromPlainText,
        ActionID = CliActionID.fmi_from_plaintext,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionFmiToPlainText = new()
    {
        Description = "Create a FMI binary file from FMI-plaintext.",
        Action = CliActions.FmiToPlainText,
        ActionID = CliActionID.fmi_to_plaintext,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    #endregion

    #region Ghost

    public static readonly GfzCliAction ActionGciExtractGhost = new()
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

    public static readonly GfzCliAction ActionGmaPatchSubmeshRenderFlags = new()
    {
        Description = "Patch render flags on model submesh.",
        Action = CliActions.PatchSubmeshRenderFlags,
        ActionID = CliActionID.gma_patch_submesh_render_flags,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.PS,
        RequiredArguments = [
            GfzCliArgumentDB.Name_GMA,
            GfzCliArgumentDB.Value_GMA],
        OptionalArguments = [
            GfzCliArgumentDB.SetFlagsOff,
            ],
    };

    #endregion

    #region

    public static readonly GfzCliAction ActionIOGma = new()
    {
        Description = "Round-trip serialize GMA files.",
        Action = CliActions.InOutGMA,
        ActionID = CliActionID.io_gma,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionIOTpl = new()
    {
        Description = "Round-trip serialize TPL files.",
        Action = CliActions.InOutTPL,
        ActionID = CliActionID.io_tpl,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionIOScene = new()
    {
        Description = "Round-trip serialize COLI_COURSE (scene) files.",
        Action = CliActions.InOutScene,
        ActionID = CliActionID.io_scene,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.OPRS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    // TODO: probably belongs in ActionsColiCourse
    public static readonly GfzCliAction ActionIOSceneNullComment = new()
    {
        Description = "Patch COLI_COURSE (scene) to null out auto-generate timestamp comment to help diff-ing.",
        Action = CliActions.PatchSceneNullComment,
        ActionID = CliActionID.io_scene_null_comment,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.PS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    #endregion

    #region ISO

    public static readonly GfzCliAction ActionIsoExtract = new()
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

    public static readonly GfzCliAction ActionIsoExtractFiles = new()
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

    public static readonly GfzCliAction ActionIsoExtractSystem = new()
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

    public static readonly GfzCliAction ActionLogStageAll = new()
    {
        Description = "Create all possible analysis .TSVs of COLI_COURSE stage files.",
        Action = CliActions.LogStageAll,
        ActionID = CliActionID.log_stage_all,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Directory,
        IsOutputOptional = false,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionLogGmaAll = new()
    {
        Description = "Create all possible analysis .TSVs of GMA model files.",
        Action = CliActions.LogGmaAll,
        ActionID = CliActionID.log_gma_all,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Directory,
        IsOutputOptional = false,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    // TODO: for each one individually
    public static readonly GfzCliAction ActionLogStageTrackKeyablesAll = new()
    {
        Description = "Create a .tsv log of track keyables from COLI_COURSE stage files.",
        Action = CliActions.LogStageTrackKeyables,
        ActionID = CliActionID.log_stage_track_keyables,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Directory,
        IsOutputOptional = false,
        FileProcessArgs = CliFileProcessArg.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    #endregion

    #region

    public static readonly GfzCliAction ActionLZCompress = new()
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

    public static readonly GfzCliAction ActionLZDecompress = new()
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

    public static readonly GfzCliAction ActionPatchBgm = new()
    {
        Description = "Set the background music for a specific stage index.",
        Action = CliActions.PatchSetBgm,
        ActionID = CliActionID.fzrel_set_bgm,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [
            GfzCliArgumentDB.BgmIndex,
            GfzCliArgumentDB.StageIndex,
            ],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionPatchBgmFinalLap = new()
    {
        Description = "Set the final lap background music for a specific stage index.",
        Action = CliActions.PatchSetBgmFinalLap,
        ActionID = CliActionID.fzrel_set_bgmfl,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [
            GfzCliArgumentDB.BgmFinalLapIndex,
            GfzCliArgumentDB.StageIndex,
            ],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionPatchBgmBoth = new()
    {
        Description = "Set both default and final lap background music for a specific stage index.",
        Action = CliActions.PatchSetBgmAndBgmFinalLap,
        ActionID = CliActionID.fzrel_set_bgm_bgmfl,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [
            GfzCliArgumentDB.BgmIndex,
            GfzCliArgumentDB.BgmFinalLapIndex,
            GfzCliArgumentDB.StageIndex,
            ],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionPatchSetCourseDifficulty = new()
    {
        Description = "Set course difficulty star rating for a specific stage.",
        Action = CliActions.PatchSetCourseDifficulty,
        ActionID = CliActionID.fzrel_set_course_difficulty,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [
            GfzCliArgumentDB.StageIndex,
            GfzCliArgumentDB.Difficulty,
            ],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionPatchSetCourseName = new()
    {
        Description = "Set course name for a specific stage index.",
        Action = CliActions.PatchSetCourseName,
        ActionID = CliActionID.fzrel_set_course_name,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [
            GfzCliArgumentDB.StageIndex,
            GfzCliArgumentDB.Value_CourseName,
            ],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionPatchClearAllCourseNames = new()
    {
        Description = "Clear all names in course name table.",
        Action = CliActions.PatchClearAllCourseNames,
        ActionID = CliActionID.fzrel_clear_all_course_names,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionPatchClearUnusedCourseNames = new()
    {
        Description = "Clear all unused course names in course name table.",
        Action = CliActions.PatchClearUnusedCourseNames,
        ActionID = CliActionID.fzrel_clear_unused_course_names,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [GfzCliArgumentDB.Value_CourseName],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionPatchSetCourseVenueIndex = new()
    {
        Description = "Set course venue for a specific stage index.",
        Action = CliActions.PatchSetCourseVenueIndex,
        ActionID = CliActionID.fzrel_set_course_venue,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [
            GfzCliArgumentDB.StageIndex,
            GfzCliArgumentDB.VenueIndex,
            ],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionPatchSetVenueName = new()
    {
        Description = "Set venue name for a specific venue index.",
        Action = CliActions.PatchSetVenueName,
        ActionID = CliActionID.fzrel_set_venue_name,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [
            GfzCliArgumentDB.VenueIndex,
            GfzCliArgumentDB.Value_VenueName,
            ],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionPatchClearAllVenueNames = new()
    {
        Description = "Clear all names in venue name table.",
        Action = CliActions.PatchClearAllVenueNames,
        ActionID = CliActionID.fzrel_clear_all_venue_names,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionPatchClearUnusedVenueNames = new()
    {
        Description = "Clear all unused course names in course name table.",
        Action = CliActions.PatchClearUnusedVenueNames,
        ActionID = CliActionID.fzrel_clear_unused_venue_names,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [GfzCliArgumentDB.Value_VenueName],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionPatchSetCarData = new()
    {
        Description = "Set \"graph console performance settings\" machine stats.",
        Action = CliActions.PatchSetCarData,
        ActionID = CliActionID.fzrel_set_cardata,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [GfzCliArgumentDB.Value_CarData],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionPatchMachineRating = new()
    {
        Description = "Set machine letter ratings (SABCDE).",
        Action = CliActions.PatchMachineRating,
        ActionID = CliActionID.fzrel_set_machine_rating,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [
            GfzCliArgumentDB.PilotNumber,
            GfzCliArgumentDB.Value_MachineRating,
            ],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionPatchMaxSpeed = new()
    {
        Description = "Patch vehicle max speed.",
        Action = CliActions.PatchMaxSpeed,
        ActionID = CliActionID.fzrel_set_max_speed,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [],
        OptionalArguments = [GfzCliArgumentDB.Value_MaxSpeed],
    };

    public static readonly GfzCliAction ActionPatchSetCupCourse = new()
    {
        Description = "Set an individual stage reference in a cup.",
        Action = CliActions.PatchSetCupCourse,
        ActionID = CliActionID.fzrel_set_cup_course,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.PRS,
        RequiredArguments = [
            GfzCliArgumentDB.Cup,           // cup to modify
            GfzCliArgumentDB.CupCourseIndex, // stage in cup to modify 0-5 (count: 6)
            GfzCliArgumentDB.StageIndex,    // stage index to use
            ],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionDecryptLineREL = new()
    {
        Description = "Decrypt line__.bin to line__.rel file.",
        Action = CliActions.DecryptLineRel,
        ActionID = CliActionID.fzrel_decrypt,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.OPRS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionEncryptLineREL = new()
    {
        Description = "Encrypt line__.rel to line__.bin file.",
        Action = CliActions.EncryptLineRel,
        ActionID = CliActionID.fzrel_encrypt,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        FileProcessArgs = CliFileProcessArg.OPRS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    #endregion

    public static readonly GfzCliAction[] GfzCliActions =
    [
        // Program
        ActionNone,
        ActionList,
        ActionUsage,
        // ARC
        ActionArcPack,
        ActionArcUnpack,
        // ASSET LIBRARY
        ActionAssetGenerateLibrary,
        ActionAssetCustomMipmapGxtex,
        ActionAssetGmarefPack,
        ActionAssetImageToGxtex,
        ActionAssetTplrefPack,
        ActionAssetTplUnpack,
        // CAMERA
        ActionCameraLivecamFromTSV,
        ActionCameraLivecamToTSV,
        // CARDATA
        ActionCarDataFromTSV,
        ActionCarDataToTSV,
        // COLICOURSE
        ActionColicoursePatchFog,
        ActionColicoursePatchObjectRenderFlags,
        // ENCODE TEXT
        ActionEncodeBytesToShiftJis,
        ActionEncodeWindows1252ToShiftJis,
        // EMBLEM
        ActionEmblemGciFromImage,
        ActionEmblemGciToImage,
        ActionEmblemsBinFromImages,
        ActionEmblemsBinToImages,
        // FMI
        ActionFmiFromPlainText,
        ActionFmiToPlainText,
        // GCI
        ActionGciExtractGhost,
        // GMA
        ActionGmaPatchSubmeshRenderFlags,
        // ISO
        ActionIsoExtract,
        ActionIsoExtractFiles,
        ActionIsoExtractSystem,
        // IO: IN-OUT TESTS
        ActionIOGma,
        ActionIOScene,
        ActionIOSceneNullComment,
        ActionIOTpl,
        // line__.rel
        ActionDecryptLineREL,
        ActionEncryptLineREL,
        ActionPatchClearAllCourseNames,
        ActionPatchClearAllVenueNames,
        ActionPatchClearUnusedCourseNames,
        ActionPatchClearUnusedVenueNames,
        ActionPatchBgm,
        ActionPatchBgmFinalLap,
        ActionPatchBgmBoth,
        ActionPatchSetCarData,
        ActionPatchSetCourseName,
        ActionPatchSetCupCourse,
        ActionPatchMachineRating,
        ActionPatchMaxSpeed,
        ActionPatchSetCourseDifficulty,
        ActionPatchSetCourseVenueIndex,
        ActionPatchSetVenueName,
        // Log
        ActionLogGmaAll,
        ActionLogStageAll,
        ActionLogStageTrackKeyablesAll,
        // LZ
        ActionLZCompress,
        ActionLZDecompress,
    ];


}
