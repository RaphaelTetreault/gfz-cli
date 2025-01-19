using System;

namespace Manifold.GFZCLI;

/// <summary>
///     List of all possible actions in this CLI program.
/// </summary>
public enum CliActionID
{
    none,
    list,
    usage,

    arc_unpack,
    arc_pack,
    auto_rename_gci,
    cardata_from_tsv,
    cardata_to_tsv,
    colicourse_patch_fog,
    colicourse_patch_object_render_flags,
    extract_iso,
    encode_bytes_to_shift_jis,
    encode_windows_to_shift_jis,
    emblem_gci_from_image,
    emblem_gci_to_image,
    emblems_bin_from_images,
    emblems_bin_to_images,
    fmi_from_plaintext,
    fmi_to_plaintext,
    generate_asset_library,
    gci_extract_ghost,
    gma_patch_submesh_render_flags,
    io_gma,
    io_scene,
    io_scene_patch,
    io_tpl,
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
    linerel_set_max_speed,
    linerel_set_course_venue,
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