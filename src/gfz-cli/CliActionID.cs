﻿using System;

namespace Manifold.GFZCLI;

/// <summary>
///     List of all possible actions in this CLI program.
/// </summary>
public enum CliActionID
{
    /// <summary>
    ///     No action selected.
    /// </summary>
    none,

    /// <summary>
    ///     List out all actions in brief.
    /// </summary>
    list,

    /// <summary>
    ///     Prints out action usage.
    /// </summary>
    usage,

    /// <summary>
    ///     Pack directory to .arc file.
    /// </summary>
    arc_unpack,

    /// <summary>
    ///     Unpack .arc file into directory of contents.
    /// </summary>
    arc_pack,

    /// <summary>
    ///     NOT IMPLEMENTED
    /// </summary>
    auto_rename_gci,

    /// <summary>
    ///     Create cardata.lz from input cardata TSV.
    /// </summary>
    cardata_from_tsv,

    /// <summary>
    ///     Create cardata TSV from input cardata.lz.
    /// </summary>
    cardata_to_tsv,

    colicourse_patch_fog,

    colicourse_patch_object_render_flags,


    extract_iso,

    /// <summary>
    ///     Encode a byte array into a Shift-JIS string.
    /// </summary>
    encode_bytes_to_shift_jis,

    /// <summary>
    ///     Encode a Windows 1252 string into a Shift-JIS string.
    /// </summary>
    encode_windows_to_shift_jis,

    /// <summary>
    ///     Extract images from GCI emblem save files.
    /// </summary>
    emblem_gci_from_image,

    /// <summary>
    ///     Create GCI emblem save files from images.
    /// </summary>
    emblem_gci_to_image,

    /// <summary>
    ///     Compile an emblem binary archive from multiple images
    /// </summary>
    emblems_bin_from_images,

    /// <summary>
    ///     Extract images from emblem binary archives.
    /// </summary>
    emblems_bin_to_images,


    fmi_from_plaintext,


    fmi_to_plaintext,

    /// <summary>
    ///     Create an asset library for extracted game ISO.
    /// </summary>
    /// <remarks>
    ///     Output not optional. Input is directory, ideally root of ISO directory.
    /// <list type="bullet">
    ///     <item>
    ///         <term>TPL</term>
    ///         <description>
    ///             Creates .PNG and .GXTEX files for each image in TPLs. Duplicates are
    ///             not written using CRC32 hashes of the image data.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>GMA</term>
    ///         <description>
    ///             Creates .GMAREF files which map GMA to individual GCMF files. Creates 
    ///             .GCMFX files for each GCMF in models. Duplicates are not written using
    ///             CRC32 hashes of the model data.
    ///         </description>
    ///     </item>
    /// </list>
    /// </remarks>
    [Action(CliActionIO.Directory, CliActionIO.Directory, CliActionOption.O, outputOptional: false)]
    generate_asset_library,


    gci_extract_ghost,


    gma_patch_submesh_render_flags,

    io_gma,
    io_scene,
    io_scene_patch,
    io_tpl,

    live_camera_stage_to_tsv,
    live_camera_stage_from_tsv,

    lz_compress,
    lz_decompress,

    linerel_clear_all_course_names,
    linerel_clear_all_venue_names,
    linerel_clear_unused_course_names,
    linerel_clear_unused_venue_names,
    linerel_decrypt,
    linerel_encrypt,
    linerel_set_bgm,
    linerel_set_bgmfl,
    linerel_set_bgm_bgmfl,
    linerel_set_cardata,
    linerel_set_course_difficulty,
    linerel_set_course_name,
    linerel_set_cup_course,
    linerel_set_machine_rating,

    
    /// <summary>
    ///     Override the game's internal max speed cap.
    /// </summary>
    /// <remarks>
    ///     The game' max speed is 9990 km/h. Calling this action without an
    ///     argument will set the max speed cap to positive infinity.
    /// </remarks>
    [Action(CliActionIO.None, CliActionIO.None, CliActionOption.R_SerializationRegion, specialOptions: ActionExOptions.linerel_set_max_speed)]
    linerel_set_max_speed,

    linerel_set_venue,
    linerel_set_venue_name,

    tpl_generate_mipmaps,
    tpl_pack,
    tpl_unpack,
}

/// <summary>
///     Metadata for <see cref="ActionAttribute"/> in <see cref="CliActionID"/>.
/// </summary>
internal static class ActionExOptions
{
    public const string OptionalsSeparator = "\n\t\t";

    public const string encode_bytes_to_shift_jis =
        $"--{IOptionsLineRel.Args.Value} <hex-string>";

    public const string encode_windows_to_shift_jis =
        $"--{IOptionsLineRel.Args.Value} <string>";

    public const string linerel_set_max_speed =
        OptionalsSeparator +
        $"[--{IOptionsLineRel.Args.Value} <max-speed=+infinity>]";

}