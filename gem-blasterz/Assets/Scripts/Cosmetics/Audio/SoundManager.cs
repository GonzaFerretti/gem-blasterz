using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class SoundManager : MonoBehaviour
{
    public Dictionary<ClipConfig, AudioSource> currentlyInitiatedSources = new Dictionary<ClipConfig, AudioSource>();

    [SerializeField]
    private ClipRepository repository;

    public ClipRepository ClipRepository => repository;
    
    public void Play(string clipName)
    {
        var clip = repository.GetClipConfig(clipName);
        if (!clip.HasValue)
            return;
        
        Play(clip.Value);
    }

    public void PlayAtPos(string clipName, Vector3 position, string positionIdentifier)
    {
        var clip = repository.GetClipConfig(clipName);
        if (!clip.HasValue)
            return;

        var clipConfig = clip.Value;
        clipConfig.position = position;
        clipConfig.positionIdentifier = positionIdentifier;
        Play(clipConfig);
    }

    public void Play(ClipConfig clip)
    {
        if (currentlyInitiatedSources.ContainsKey(clip))
        {
            PlayFromExisting(clip);
        }
        else
        {
            CreateSourceAndPlay(clip);
        }
    }

    void PlayFromExisting(ClipConfig clip)
    {
        currentlyInitiatedSources[clip].Play();
    }

    void CreateSourceAndPlay(ClipConfig clip)
    {
        CreateNewSource(clip);
        PlayFromExisting(clip);
    }

    public void Stop(ClipConfig clip)
    {
        if (currentlyInitiatedSources.ContainsKey(clip))
        {
            currentlyInitiatedSources[clip].Stop();
        }
        else
        {
            Debug.LogWarning("No sound source was found using that clip");
        }
    }

    void CreateNewSource(ClipConfig clip)
    {
        var sourceGameObject = gameObject;
        if (clip.isSpatial)
        {
            sourceGameObject = new GameObject($"AudioSource: {clip.identifier}");
            sourceGameObject.transform.position = clip.position;
        }
        AudioSource newAs = sourceGameObject.AddComponent<AudioSource>();
        newAs.clip = clip.file;
        newAs.volume = clip.volume;
        newAs.loop = clip.shouldLoop;
        newAs.pitch = clip.pitch;
        //newAs.spatialBlend = clip.isSpatial ? 1 : 0;
        currentlyInitiatedSources.Add(clip, newAs);
    }
}