namespace Manifold.GFZCLI
{
    public enum GfzCliAction
    {
        none,

        arc_decompress,
        arc_compress,

        auto_rename_gci,

        cardata_bin_to_tsv,
        cardata_tsv_to_bin,

        extract_iso_files,

        emblem_bin_to_image,
        emblem_gci_to_image,
        image_to_emblem_bin,
        image_to_emblem_gci,

        fmi_to_plaintext,
        fmi_from_plaintext,

        gci_extract_ghost,

        live_camera_stage_bin_to_tsv,
        live_camera_stage_tsv_to_bin,

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
        linerel_set_course_difficulty,
        linerel_set_course_name,
        linerel_set_cup_course,
        linerel_set_venue,
        linerel_set_venue_name,

        tpl_unpack,
    }
}
