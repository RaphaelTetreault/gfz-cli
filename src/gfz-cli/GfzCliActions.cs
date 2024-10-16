﻿namespace Manifold.GFZCLI
{
    public enum GfzCliAction
    {
        none,
        usage,

        [Action(ActionIO.FileIn | ActionIO.DirectoryOut)]
        arc_unpack,

        [Action(ActionIO.DirectoryIn | ActionIO.FileOut)]
        arc_pack,

        auto_rename_gci, // NOT IMPLEMENTED

        [Action(ActionIO.FileInOut)]
        cardata_from_tsv,

        [Action(ActionIO.FileInOut)]
        cardata_to_tsv,

        [Action(ActionIO.FileInOut)]
        colicourse_patch_fog,

        [Action(ActionIO.FileInOut)]
        colicourse_patch_object_render_flags,

        [Action(ActionIO.FileIn | ActionIO.DirectoryOut)]
        extract_iso,

        emblem_gci_from_image,
        emblem_gci_to_image,
        emblems_bin_from_images,
        emblems_bin_to_images,

        fmi_from_plaintext,
        fmi_to_plaintext,

        [Action(ActionIO.DirectoryIn | ActionIO.DirectoryOut)]
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
        linerel_set_venue,
        linerel_set_venue_name,

        tpl_generate_mipmaps,
        tpl_pack,
        tpl_unpack,
    }
}
