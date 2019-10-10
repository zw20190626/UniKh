/** EW_CSP.cs
 *  Author:         bagaking <kinghand@foxmail.com>
 *  CreateTime:     2019/10/09 21:40:09
 *  Copyright:      (C) 2019 - 2029 bagaking, All Rights Reserved
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UniKh.core;
using UniKh.extensions;
using System;
using UnityEditor;
using UniKh.utils;

namespace UniKh.editor {
    public class EW_CSP : EWBase<EW_CSP> {

        [MenuItem("UniKh/CSP_Monitor")]
        public static void ShowDialog() {
            GetWindow("UniKh - CSP_Monitor");
        }

        public override bool Initial() {
            return true;
        }

        public class ProcStatistic {
            public string Tag = "_";
            public long TotalCpuTimeCostMS = 0;
            public long TotalFrameCount = 0;
        }

        public static Dictionary<int, ProcStatistic> procStatistics = new Dictionary<int, ProcStatistic>();

        public ProcStatistic GetProcStatistic(int id) {
            if (!procStatistics.ContainsKey(id)) {
                procStatistics.Add(id, new ProcStatistic());
            }
            return procStatistics[id];
        }

        public GUILayoutTogglePanel tpProcInAction = new GUILayoutTogglePanel("Proc in action", true);
        public GUILayoutTogglePanel tpProcFinished = new GUILayoutTogglePanel("Proc finished");

        public override void GUIProc(Event e) {
            if (!Application.isPlaying) {
                EditorGUILayout.LabelField("UniKh/CSP can only be accessed while the program is running ...");
                return;
            }

            if(!CSP.Inst) {
                EditorGUILayout.LabelField("UniKh/CSP cre not loaded ...");
                return;
            }

            EditorGUILayout.LabelField("Statistics:");
            EditorGUILayout.LabelField("|  Procs\t| Frames\t| MS\t| Ticks\t| Updates");
            EditorGUILayout.LabelField($"| {CSP.Inst.procLst.Count}\t| {CSP.Inst.MonitExecutedInFrame}\t| {CSP.Inst.MonitTickTimeCost}\t| {CSP.Inst.TotalTicks}\t| {CSP.Inst.MonitTotalUpdates}");

            var array = new int[procStatistics.Count];
            procStatistics.Keys.CopyTo(array, 0);
            var disabledProcs = new List<int>(array);

            tpProcInAction.Draw(CSP.Inst.procLst.Count.ToString(), () => {
                foreach (var proc in CSP.Inst.procLst) {
                    var st = GetProcStatistic(proc.ID);
                    disabledProcs.Remove(proc.ID);
                    st.Tag = proc.Tag;
                    st.TotalCpuTimeCostMS += proc.MonitTickTimeCost;
                    st.TotalFrameCount += proc.MonitTickFrameCount;

                    var OpCurr = proc.GetOpCurr();
                    EditorGUILayout.LabelField(
                        SGen.New[proc.ID][". #"][proc.Tag]
                        ["  At:"][proc.ExecutedTime]
                        ["  Frames:"][proc.MonitTickFrameCount]['/'][st.TotalFrameCount]
                        ["  MS:"][proc.MonitTickTimeCost]['/'][st.TotalCpuTimeCostMS]
                        ["  Waiting:"][OpCurr == null ? "(null)" : OpCurr.ToString()].End);

                    var StackTrace = SGen.New["Stack: "];
                    proc.ProcStack.ForEach((layer, ind) => {
                        StackTrace = StackTrace['/'][layer.Current == null ? "null" : layer.Current.ToString()];
                    });
                    EditorGUILayout.LabelField(StackTrace.End);
                }
            });


            tpProcFinished.Draw(disabledProcs.Count.ToString(), () => {
                foreach (var procID in disabledProcs) {
                    var st = GetProcStatistic(procID);
                    EditorGUILayout.LabelField(
                        SGen.New[procID][". #"][st.Tag]
                        ["  Frames:"][st.TotalFrameCount]
                        ["  MS:"][st.TotalCpuTimeCostMS]
                        .End);
                }
            });
            
        }


        //private bool using_corou_in_editor = false;
        public void Update() {
            //if (!Application.isPlaying && using_corou_in_editor) {
            //    Corou.TriggerTick();
            //}
            Repaint();
        }

    }
}

public class GUILayoutTogglePanel {

    public string title;
    public bool state;
    public GUILayoutTogglePanel(string title, bool initState = false) {
        this.title = title;
        this.state = initState;
    }

    public void Draw(string appendTitle, Action DeawContent) {
        GUILayout.Space(3f);

        var cBgOrigin = GUI.backgroundColor;
        GUI.backgroundColor = new Color(2f, 2f, 0.9f);
        state = GUILayout.Toggle(state, SGen.New[state ? "\u25B2" : "\u25BC"]["<b> <size=11>"][title][' '][appendTitle]["</size></b>"].End, "dragtab", GUILayout.MinWidth(20f));

        GUILayout.Space(2f);
        GUI.backgroundColor = cBgOrigin;

        if (state && null != DeawContent) {
            DeawContent();
        } else {
            GUILayout.Space(3f);
        }

    }

    public void Draw(Action DeawContent) {
        this.Draw("", DeawContent);
    }
}