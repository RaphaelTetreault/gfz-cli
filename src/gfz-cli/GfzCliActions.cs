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

        live_camera_stage_bin_to_tsv,
        live_camera_stage_tsv_to_bin,

        lz_compress,
        lz_decompress,

        tpl_unpack,
    }
}
