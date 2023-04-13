using System;
using System.Collections.Generic;
using System.Xml;
using PRProjMulticamCreator.Models;

namespace PRProjMulticamCreator.Services;

public class NodeService
{
    public void UpdateMulticamTrack(XmlDocument prproj, List<FrameModel> frames)
    {
        // get original nodes for cloning
        var originalVideoClipNode = prproj.SelectSingleNode("(//VideoClip[Clip/IsMulticam='true'])[1]");
        var originalVideoClipObjectId = originalVideoClipNode?.Attributes?["ObjectID"]?.Value;
        var originalSubClipNode = prproj.SelectSingleNode($"//SubClip[Clip[@ObjectRef='{originalVideoClipObjectId}']]");
        var originalSubClipObjectId = originalSubClipNode?.Attributes?["ObjectID"]?.Value;
        var originalVideoClipTrackItemNode = prproj.SelectSingleNode($"//VideoClipTrackItem[ClipTrackItem[SubClip[@ObjectRef='{originalSubClipObjectId}']]]");
        var originalVideoClipTrackItemObjectId = originalVideoClipTrackItemNode?.Attributes?["ObjectID"]?.Value;
        var originalComponentsNode = originalVideoClipTrackItemNode?.SelectSingleNode(".//Components");
        var originalComponentsNodeObjectId = originalComponentsNode?.Attributes?["ObjectRef"]?.Value;
        var originalVideoComponentChainNode = prproj.SelectSingleNode($"//VideoComponentChain[@ObjectID='{originalComponentsNodeObjectId}']");
        var originalTrackItemNode = prproj.SelectSingleNode($"//TrackItem[@ObjectRef='{originalVideoClipTrackItemObjectId}']");

        // start cloning
        var i = 0;
        foreach (var frame in frames)
        {
            var videoClipObjectId = 10000 + i;
            var subClipObjectId = 20000 + i;
            var videoComponentChainObjectId = 30000 + i;
            var videoClipTrackItemObjectId = 40000 + i;

            CloneVideoClipNode(originalVideoClipNode, videoClipObjectId, frame.InPoint, frame.OutPoint, frame.TrackIndex);
            CloneSubClipNode(originalSubClipNode, subClipObjectId, videoClipObjectId);
            CloneVideoComponentChainNode(originalVideoComponentChainNode, videoComponentChainObjectId);
            CloneVideoClipTrackItemNode(originalVideoClipTrackItemNode, videoClipTrackItemObjectId, videoComponentChainObjectId, subClipObjectId, frame.InPoint, frame.OutPoint);
            CloneTrackItemNode(originalTrackItemNode, i, videoClipTrackItemObjectId);

            i += 1;
        }

        // remove original nodes
        originalVideoClipNode?.ParentNode?.RemoveChild(originalVideoClipNode);
        originalSubClipNode?.ParentNode?.RemoveChild(originalSubClipNode);
        originalVideoClipTrackItemNode?.ParentNode?.RemoveChild(originalVideoClipTrackItemNode);
        originalTrackItemNode?.ParentNode?.RemoveChild(originalTrackItemNode);
    }

    private void CloneVideoClipNode(
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

    private void CloneSubClipNode(
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

    private void CloneVideoComponentChainNode(
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

    private void CloneVideoClipTrackItemNode(
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

    private void CloneTrackItemNode(
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