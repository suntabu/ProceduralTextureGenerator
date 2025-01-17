﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

namespace PTG
{
    public class AmbientOcclusionNode : NodeBase
    {
        RenderTexture source;
        ConnectionPoint inPoint;
        ConnectionPoint outPoint;

        public AmbientOcclusionNode()
        {
            title = "Occlusion";
        }

        public void OnEnable()
        {
            InitTexture();
            shader = (ComputeShader)Resources.Load("AmbientOcclusion");
            kernel = shader.FindKernel("AmbientOcclusion");
            texture.enableRandomWrite = true;
            texture.Create();
        }

        public void Init(Vector2 position, float width, float height, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint, Action<NodeBase> OnClickRemoveNode, NodeEditorWindow editor)
        {
            rect = new Rect(position.x, position.y, width, height);
            this.editor = editor;
            outPoint = new ConnectionPoint(this, ConnectionType.Out, outPointStyle, OnClickOutPoint);
            if (outPoints == null)
            {
                outPoints = new List<ConnectionPoint>();
            }

            outPoints.Add(outPoint);

            inPoint = new ConnectionPoint(this, ConnectionType.In, inPointStyle, OnClickInPoint);

            if (inPoints == null)
            {
                inPoints = new List<ConnectionPoint>();
            }

            inPoints.Add(inPoint);

            OnRemoveNode = OnClickRemoveNode;
        }

        public override void Draw()
        {
            base.Draw();
            GUILayout.BeginArea(rect);
            if (texture != null)
            {
                GUI.DrawTexture(new Rect(0 + rect.width / 8, 0 + rect.height / 5, rect.width - rect.width / 4, rect.height - rect.height / 4), texture);
            }
            GUILayout.EndArea();
        }

        public override void DrawInspector()
        {
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Warning: This node performs large computations on the GPU.", MessageType.Warning);

            base.DrawInspector();
        }

        public override void Compute(bool selfcompute = false)
        {
            NodeBase n = null;
            if (inPoint.connections.Count != 0)
            {
                n = inPoint.connections[0].outPoint.node;
            }

            if (n != null)
            {
                if (n.GetTexture() != null)
                {
                    source = n.GetTexture();
                    if(n.ressolution!=this.ressolution)
                    {
                        ChangeRessolution(n.ressolution.x);
                    }
                }
            }
            if (selfcompute)
            {
                if (source != null && texture != null)
                {
                    if (shader != null)
                    {
                        if (ressolution.x / 8 > 0)
                        {
                            shader.SetTexture(kernel, "Result", texture);
                            shader.SetTexture(kernel, "source", source);
                            shader.SetFloat("ressolution", ressolution.x);
                            shader.SetFloat("Samples", 16f);
                            shader.Dispatch(kernel, ressolution.x / 8, ressolution.y / 8, 1);
                        }
                    }
                }
            }

            if (outPoint.connections != null)
            {
                for (int i = 0; i < outPoint.connections.Count; i++)
                {
                    outPoint.connections[i].inPoint.node.Compute(true);
                }
            }
        }
    }
}
