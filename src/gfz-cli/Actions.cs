namespace Manifold.GFZCLI;

/// <summary>
///     List of all possible actions in this CLI program.
/// </summary>
public enum Actions
{
    none,
    usage,

    /// <summary>
    ///     Requires path input, directory output is optional. ARC compatible between AX/GX, all regions.
    /// </summary>
    [Action(ActionIO.Path, ActionIO.Directory, ActionOption.OPS)]
    arc_unpack,

    /// <summary>
    ///     Requires directory input, directory output is optional. ARC compatible between AX/GX, all regions.
    /// </summary>
    [Action(ActionIO.Directory, ActionIO.Directory, ActionOption.OPS)]
    arc_pack,


    auto_rename_gci, // NOT IMPLEMENTED

    //[Action(ActionIO.FileInOut, ActionOption.FOPS)]
    cardata_from_tsv,
    //[Action(ActionIO.FileInOut, ActionOption.FOPS)]
    cardata_to_tsv,

    //[Action(ActionIO.FileInOut, ActionOption.FOPS)]
    colicourse_patch_fog,
    //[Action(ActionIO.FileInOut, ActionOption.FOPS)]
    colicourse_patch_object_render_flags,

    //[Action(ActionIO.FileIn | ActionIO.DirectoryOut, ActionOption.OPS)]
    extract_iso,

    /// <summary>
    ///     Takes in argument via <see cref="Options.Value"/>.
    /// </summary>
    [Action(ActionIO.None, ActionIO.None, ActionOption.None, specialOptions: $"--{IOptionsLineRel.Args.Value} <hex-string>")]
    encode_bytes_to_shift_jis,

    /// <summary>
    ///     Takes in argument via <see cref="Options.Value"/>.
    /// </summary>
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
    ///     Requires directory file (to patch), directory output is optional. ARC compatible between AX/GX, all regions.
    /// </summary>
    /// <remarks>
    ///     The game' max speed is 9990 km/h. Calling this action with by default set it to +infinity if no arguments
    ///     are specified.
    /// </remarks>
    [Action(ActionIO.None, ActionIO.None, ActionOption.SerializationRegion, specialOptions: $"[--{IOptionsLineRel.Args.Value} <max-speed>]")]
    linerel_set_max_speed,

    linerel_set_venue,
    linerel_set_venue_name,

    tpl_generate_mipmaps,
    tpl_pack,
    tpl_unpack,
}
