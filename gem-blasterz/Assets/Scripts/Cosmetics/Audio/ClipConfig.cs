using System;
using UnityEngine;

[Serializable]
public struct ClipConfig : IEquatable<ClipConfig>
{
    public string identifier;
    public AudioClip file;
    [Range(0,1)]
    public float volume;
    public float pitch;
    public bool shouldLoop;
    public bool isSpatial;
    
    [Header("Run-time only")]
    public Vector3 position;
    public string positionIdentifier;

    public bool Equals(ClipConfig other)
    {
        return identifier == other.identifier && Equals(file, other.file) && volume.Equals(other.volume) && shouldLoop == other.shouldLoop && isSpatial == other.isSpatial && positionIdentifier == other.positionIdentifier && pitch.Equals(other.pitch);
    }

    public override bool Equals(object obj)
    {
        return obj is ClipConfig other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (identifier != null ? identifier.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (file != null ? file.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ volume.GetHashCode();
            hashCode = (hashCode * 397) ^ pitch.GetHashCode();
            hashCode = (hashCode * 397) ^ shouldLoop.GetHashCode();
            hashCode = (hashCode * 397) ^ isSpatial.GetHashCode();
            hashCode = (hashCode * 397) ^ (positionIdentifier != null ? positionIdentifier.GetHashCode() : 0);
            return hashCode;
        }
    }
}