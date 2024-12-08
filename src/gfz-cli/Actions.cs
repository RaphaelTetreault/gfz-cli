namespace Manifold.GFZCLI;

/// <summary>
///     List of all possible actions in this CLI program.
/// </summary>
public enum Actions
{
    none,
    usage,

    /// <summary>
    ///     Pack directory to .arc file.
    /// </summary>
    /// <remarks>
    ///     Requires path input, directory output is optional. ARC compatible between AX/GX, all regions.
    /// </remarks>
    [Action(ActionIO.Path, ActionIO.Directory, ActionOption.OPS)]
    arc_unpack,

    /// <summary>
    ///     Unpack .arc file into directory of contents.
    /// </summary>
    /// <remarks>
    ///     Requires directory input, directory output is optional. ARC compatible between AX/GX, all regions.
    /// </remarks>
    [Action(ActionIO.Directory, ActionIO.Directory, ActionOption.OPS)]
    arc_pack,


    auto_rename_gci, // NOT IMPLEMENTED

    cardata_from_tsv,
    cardata_to_tsv,

    colicourse_patch_fog,
    colicourse_patch_object_render_flags,

    extract_iso,

    /// <summary>
    ///     Encode a byte array into a Shift-JIS string.
    /// </summary>
    /// <remarks>
    ///     Takes in argument via <see cref="Options.Value"/>.
    /// </remarks>
    [Action(ActionIO.None, ActionIO.None, ActionOption.None, specialOptions: $"--{IOptionsLineRel.Args.Value} <hex-string>")]
    encode_bytes_to_shift_jis,

    /// <summary>
    ///     Encode a Windows 1252 string into a Shift-JIS string.
    /// </summary>
    /// <remarks>
    ///     Takes in argument via <see cref="Options.Value"/>.
    /// </remarks>
    [Action(ActionIO.None, ActionIO.None, ActionOption.None, specialOptions: $"--{IOptionsLineRel.Args.Value} <string>")]
    encode_windows_to_shift_jis,

    emblem_gci_from_image,
    emblem_gci_to_image,
    emblems_bin_from_images,
    emblems_bin_to_images,

    fmi_from_plaintext,
    fmi_to_plaintext,

    //[Action(ActionIO.DirectoryIn | ActionIO.DirectoryOut)]
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
    [Action(ActionIO.None, ActionIO.None, ActionOption.SerializationRegion, specialOptions: $"[--{IOptionsLineRel.Args.Value} <max-speed>]")]
    linerel_set_max_speed,

    linerel_set_venue,
    linerel_set_venue_name,

    tpl_generate_mipmaps,
    tpl_pack,
    tpl_unpack,
}
