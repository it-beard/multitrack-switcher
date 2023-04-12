using System;
using System.Xml;

namespace PRProjMulticamCreator.Services;

public static class NodeService
{
    public static void CloneVideoClipNode(
        XmlNode originalNode,
        int objectId,
        long inPointValue,
        long outPointValue,
        int selectedTrackIndexValue)
    {
        // clone node
        XmlNode node = originalNode.CloneNode(true);

        // update values
        var objectIdAttribute = node.Attributes?["ObjectID"];
        objectIdAttribute.Value = objectId.ToString();
        var intPoint = node.SelectSingleNode("Clip/InPoint");
        intPoint.InnerText = inPointValue.ToString();
        var outPoint = node.SelectSingleNode("Clip/OutPoint");
        outPoint.InnerText = outPointValue.ToString();
        var selectedTrackIndex = node.SelectSingleNode("Clip/SelectedTrackIndex");
        selectedTrackIndex.InnerText = selectedTrackIndexValue.ToString();
        var clipId = node.SelectSingleNode("Clip/ClipID");
        clipId.InnerText = Guid.NewGuid().ToString();

        // insert node in the end
        originalNode.ParentNode?.InsertAfter(node, originalNode);
    }

    public static void CloneSubClipNode(
        XmlNode originalNode,
        int objectId,
        int clipObjectRef)
    {
        // clone node
        var node = originalNode.CloneNode(true);

        // update values
        var objectIdAttribute = node.Attributes?["ObjectID"];
        objectIdAttribute.Value = objectId.ToString();

        var clip = node.SelectSingleNode("Clip");
        var clipObjectRefAttribute = clip?.Attributes?["ObjectRef"];
        clipObjectRefAttribute.Value = clipObjectRef.ToString();

        // insert node
        originalNode.ParentNode?.InsertAfter(node, originalNode);
    }

    public static void CloneVideoComponentChainNode(
        XmlNode originalNode,
        int objectId)
    {
        // clone node
        var node = originalNode.CloneNode(true);

        // update values
        var objectIdAttribute = node.Attributes?["ObjectID"];
        objectIdAttribute.Value = objectId.ToString();

        // insert node
        originalNode.ParentNode?.InsertAfter(node, originalNode);
    }

    public static void CloneVideoClipTrackItemNode(
        XmlNode originalNode,
        int objectId,
        int componentsObjectRef,
        int subClipObjectRef,
        long startValue,
        long endValue)
    {
        // clone node
        var node = originalNode.CloneNode(true);

        // update values
        var objectIdAttribute = node.Attributes?["ObjectID"];
        objectIdAttribute.Value = objectId.ToString();
        var components = node.SelectSingleNode("ClipTrackItem/ComponentOwner/Components");
        var componentsObjectRefAttribute = components?.Attributes?["ObjectRef"];
        componentsObjectRefAttribute.Value = componentsObjectRef.ToString();
        var subClip = node.SelectSingleNode("ClipTrackItem/SubClip");
        var subClipObjectRefAttribute = subClip?.Attributes?["ObjectRef"];
        subClipObjectRefAttribute.Value = subClipObjectRef.ToString();
        var start = node.SelectSingleNode("ClipTrackItem/TrackItem/Start");
        start.InnerText = startValue.ToString();
        var end = node.SelectSingleNode("ClipTrackItem/TrackItem/End");
        end.InnerText = endValue.ToString();

        // insert node
        originalNode.ParentNode?.InsertAfter(node, originalNode);
    }

    public static void CloneTrackItemNode(
        XmlNode originalNode,
        int index,
        int objectRef)
    {
        // clone node
        var node = originalNode.CloneNode(true);

        // update values
        var objectRefAttribute = node.Attributes?["ObjectRef"];
        objectRefAttribute.Value = objectRef.ToString();
        var indexAttribute = node.Attributes?["Index"];
        indexAttribute.Value = index.ToString();

        // insert node
        originalNode.ParentNode?.InsertAfter(node, originalNode);
    }
}