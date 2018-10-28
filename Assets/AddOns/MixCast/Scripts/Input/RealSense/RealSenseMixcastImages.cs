using Intel.RealSense;
using UnityEngine;

public class RealSenseMixcastImages : RealSenseStreamTexture
{
    [Space]
    public Stream alignTo;

    public int decimationAmount = 2;

    Align aligner;

    // Postprocessing classes
    DecimationFilter decimationFilter;
    SpatialFilter spatialFilter;
    TemporalFilter temporalFilter;

    void OnDestroy()
    {
        OnStopStreaming();
    }

    protected override void OnStopStreaming()
    {
        if (aligner != null)
        {
            aligner.Dispose();
            aligner = null;
        }

        if (decimationFilter != null)
        {
            decimationFilter.Dispose();
            decimationFilter = null;
        }

        if (spatialFilter != null)
        {
            spatialFilter.Dispose();
            spatialFilter = null;
        }

        if (temporalFilter != null)
        {
            temporalFilter.Dispose();
            temporalFilter = null;
        }
    }

    private void InitializePostProcessingFilter()
    {
        decimationFilter = new DecimationFilter();
        spatialFilter = new SpatialFilter();
        temporalFilter = new TemporalFilter();

        // Set some reasonable defaults for now
        spatialFilter.Options[Option.HolesFill].Value = 1;
        spatialFilter.Options[Option.FilterMagnitude].Value = 3.0f;
        spatialFilter.Options[Option.FilterSmoothAlpha].Value = 0.5f;
        spatialFilter.Options[Option.FilterSmoothDelta].Value = 18.0f;

        temporalFilter.Options[Option.HolesFill].Value = 2;
        temporalFilter.Options[Option.FilterSmoothAlpha].Value = 0.6f;
        temporalFilter.Options[Option.FilterSmoothDelta].Value = 30.0f;
    }

    protected override void OnStartStreaming(PipelineProfile activeProfile)
    {
        InitializePostProcessingFilter();

        aligner = new Align(alignTo);

        StreamProfile profile = null;

        for (int i = 0; i < activeProfile.Streams.Count; i++)
        {
            if (activeProfile.Streams[i].Stream == alignTo)
                profile = activeProfile.Streams[i];
        }

        if (profile == null)
        {
            Debug.LogWarningFormat("Stream {0} not in active profile", sourceStreamType);
            return;
        }

        var videoProfile = profile as VideoStreamProfile;
        //Decimation = 2
        texture = new Texture2D(videoProfile.Width / decimationAmount, videoProfile.Height / decimationAmount, textureFormat, false, true)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = filterMode
        };
        texture.Apply();
        textureBinding.Invoke(texture);

        realSenseDevice.onNewSampleSet += OnFrameSet;
    }

    private void OnFrameSet(FrameSet frames)
    {
        using (var aligned = aligner.Process(frames))
        {
            using (var f = aligned[sourceStreamType])
            {
                using (VideoFrame vPost = f as VideoFrame)
                {
                    using (VideoFrame v1 = decimationFilter.ApplyFilter(vPost))
                    {
                        using (VideoFrame v2 = spatialFilter.ApplyFilter(v1))
                        {
                            using (VideoFrame vOut = temporalFilter.ApplyFilter(v2))
                            {
                                OnFrame(vOut);
                            }
                        }
                    }
                }
            }
        }
    }
}
