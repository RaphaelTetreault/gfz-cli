namespace Manifold.GFZCLI
{
    public enum GfzCliAction
    {
        none,

        cardata_bin_to_tsv,
        cardata_tsv_to_bin,

        live_camera_stage_bin_to_tsv,
        live_camera_stage_tsv_to_bin,

        lz_compress,
        lz_decompress,

        tpl_pack,
    }
}
