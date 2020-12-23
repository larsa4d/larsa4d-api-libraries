using System;
using System.Collections.Generic;
using System.Reflection;

namespace Larsa4D
{
    public enum LarsaAppMode { AttachToLARSA, Standalone }

    public class LarsaApp
    {
        private static LarsaAppMode Mode;
        private static object App;
        private static object Project;
        private static object Analysis;
        private static object Settings;

        private LarsaUnits _Units;
        public LarsaUnits Units { get { return _Units; } }

        private LarsaArray<LarsaUCS> _UCS;
        public LarsaArray<LarsaUCS> UCS { get { return _UCS; } }

        private LarsaArray<LarsaJoint> _Joints;
        public LarsaArray<LarsaJoint> Joints { get { return _Joints; } }

        private LarsaArray<LarsaMaterial> _Materials;
        public LarsaArray<LarsaMaterial> Materials { get { return _Materials; } }

        private LarsaArray<LarsaSection> _Sections;
        public LarsaArray<LarsaSection> Sections { get { return _Sections; } }

        private LarsaArray<LarsaMember> _Members;
        public LarsaArray<LarsaMember> Members { get { return _Members; } }

        private LarsaArray<LarsaMemberFixity> _MemberFixities;
        public LarsaArray<LarsaMemberFixity> MemberFixities { get { return _MemberFixities; } }

        private LarsaArray<LarsaPlate> _Plates;
        public LarsaArray<LarsaPlate> Plates { get { return _Plates; } }

        private LarsaArray<LarsaSpring> _Springs;
        public LarsaArray<LarsaSpring> Springs { get { return _Springs; } }

        private LarsaArray<LarsaLane> _Lanes;
        public LarsaArray<LarsaLane> Lanes { get { return _Lanes; } }

        private LarsaArray<LarsaLoadCase> _LoadCases;
        public LarsaArray<LarsaLoadCase> LoadCases { get { return _LoadCases; } }

        private LarsaArray<LarsaLoadCombination> _LoadCombinations;
        public LarsaArray<LarsaLoadCombination> LoadCombinations { get { return _LoadCombinations; } }

        private LarsaArray<LarsaGroup> _Groups;
        public LarsaArray<LarsaGroup> Groups { get { return _Groups; } }

        public LarsaApp(LarsaAppMode mode)
        {
            Mode = mode;
            this.RefreshRefs();
        }

        private void RefreshRefs()
        {
            if (Mode == LarsaAppMode.AttachToLARSA)
            {
                Type AppType = Type.GetTypeFromProgID("Larsa2000.Application");
                App = Activator.CreateInstance(AppType);
                do
                {
                    Project = GetProp(App, "project");
                } while (Project == null);

                Analysis = GetProp(App, "analysis");
                Settings = GetProp(App, "settings");
            }
            else
            {
                Type AppType = Type.GetTypeFromProgID("LarsaElements.clsProject");
                Project = Activator.CreateInstance(AppType);
                AppType = Type.GetTypeFromProgID("LarsaElements.clsAnalysisResults");
                Analysis = Activator.CreateInstance(AppType);
            }

            _Units = new LarsaUnits(GetProp(Project, "units"));
            _UCS = new LarsaArray<LarsaUCS>(GetProp(Project, "ucs"));
            _Joints = new LarsaArray<LarsaJoint>(GetProp(Project, "joints"));
            _Materials = new LarsaArray<LarsaMaterial>(GetProp(Project, "materials"));
            _Sections = new LarsaArray<LarsaSection>(GetProp(Project, "sections"));
            _Members = new LarsaArray<LarsaMember>(GetProp(Project, "members"));
            _MemberFixities = new LarsaArray<LarsaMemberFixity>(GetProp(Project, "geometry", 9));
            _Plates = new LarsaArray<LarsaPlate>(GetProp(Project, "plates"));
            _Springs = new LarsaArray<LarsaSpring>(GetProp(Project, "springs"));
            _LoadCases = new LarsaArray<LarsaLoadCase>(GetProp(Project, "primaryLoadCases"));
            _Lanes = new LarsaArray<LarsaLane>(GetProp(Project, "lanes"));
            _LoadCombinations = new LarsaArray<LarsaLoadCombination>(GetProp(Project, "combinationLoadCases"));
            _Groups = new LarsaArray<LarsaGroup>(GetProp(Project, "geoGroups"));
        }
        public void Update() { CallMethod(Project, "RaiseDataChangedEvent", new object[] { 0 }); }
        public void GraphicsZoomExtend() { CallMethod(Project, "RaiseGraphicsEvent", new object[] { 5 }); }
        public void GraphicsRedrawAll() { CallMethod(Project, "RaiseGraphicsEvent", new object[] { 0 }); }
        public void GraphicsRefresh() { CallMethod(Project, "RaiseGraphicsEvent", new object[] { 1 }); }

        public int VersionMajor() { return (int)GetProp(Project, "major", new object[] { });}
        public int VersionMinor() { return (int)GetProp(Project, "minor", new object[] { }); }
        public int VersionRevision() { return (int)GetProp(Project, "revision", new object[] { }); }

        public void New() { CallMethod(App, "NewProject", new object[] { }); }
        public bool Load(string filename) { bool ret = (bool)CallMethod(App, "LoadProject", new object[] { filename }); this.RefreshRefs(); return ret; }
        public void Save(string filename) { CallMethod(App, "SaveProjectAs", new object[] { filename, 1, this.VersionMajor(), this.VersionMinor(), this.VersionRevision() }); }
        public void SaveToFile(string filename) { CallMethod(Project, "SaveToFile", new object[] { filename, 1, this.VersionMajor(), this.VersionMinor(), this.VersionRevision(), Analysis }); }
        public void Hide() { CallMethod(App, "hideLARSA", new object[] { }); }
        public void Show() { CallMethod(App, "showLARSA", new object[] { }); }
        public void Exit() { CallMethod(App, "ExitProgram", new object[] { }); }

        public string AddPluginMenu(object plugin, string menuID, string label, int position = -1) { return (string)CallMethod(App, "addPluginMenu", new object[] { plugin, menuID, label, position }); }
        public void AddPluginTool(object plugin, string menuID, string toolID, string label, bool startGroup = false) { CallMethod(App, "addPluginTool", new object[] { plugin, menuID, toolID, label, 0, startGroup }); }
        public void RegisterPlugin(object plugin) { CallMethod(App, "registerPlugin", new object[] { plugin }); }

        private string MakeNativeMenuID(string pluginName, string toolID) { return "ID_Plugin___" + pluginName + "___" + toolID; }
        public void MenuToolShowHide(string pluginName, string toolID, bool visible)
        {
            object toolbars = CallMethod(App, "GetControl", new object[] { "MainToolbarsA" });
            object tools = GetProp(toolbars, "tools");
            object tool = CallMethod(tools, "Item", new object[] { MakeNativeMenuID(pluginName, toolID) });
            SetProp(tool, "visible", visible);
        }

        public void RunAnalysis(bool async) { CallMethod(App, "RunAnalysis", new object[] { async }); }
        public void ResultSelectCase(string ResultCaseName) { CallMethod(App, "SelectResultCase", new object[] { ResultCaseName }); }
        public bool IsSelectedResultCaseSolved()
        {
            var activeCase = GetProp(Analysis, "activeCase");
            return (bool)GetProp(activeCase, "Solved");
        }
        public object[] ResultJointDisp(int jointID) { return (object[])CallMethod(App, "DisplacementsAtJoint", new object[] { jointID }); }
        public object[] ResultJointReactions(int jointID) { return (object[])CallMethod(App, "ReactionsAtJoint", new object[] { jointID }); }
        public object[] ResultMemberForceGlobalAtStart(int memberID) { return (object[])CallMethod(App, "ForcesAtStartOfMemberGlobal", new object[] { memberID }); }
        public object[] ResultMemberForceGlobalAtEnd(int memberID) { return (object[])CallMethod(App, "ForcesAtEndOfMemberGlobal", new object[] { memberID }); }
        public object[] ResultMemberForceAtStart(int memberID) { return (object[])CallMethod(App, "ForcesAtStartOfMember", new object[] { memberID }); }
        public object[] ResultMemberForceAtEnd(int memberID) { return (object[])CallMethod(App, "ForcesAtEndOfMember", new object[] { memberID }); }
        public object[] ResultMemberForceAtMid(int memberID) { return (object[])CallMethod(App, "ForcesInMidOfMember", new object[] { memberID }); }
        public object[] ResultMemberForceAtStation(int memberID, int Station) { return (object[])CallMethod(App, "ForcesAtStationAlongMember", new object[] { memberID, Station }); }
        public object[] ResultSpringForce(int springID) { return (object[])CallMethod(App, "ForcesInSpring", new object[] { springID }); }
        public object[] ResultIsolatorForce(int isolatorID) { return (object[])CallMethod(App, "ForcesInIsolator", new object[] { isolatorID }); }
        public object[] ResultCompoundForces(double station) { return (object[])CallMethod(App, "CompoundForces", new object[] { station }); }
        public object[] ResultCompoundForcesGenDetailedReport(double station) { return (object[])CallMethod(App, "CompoundForces", new object[] { station, true }); }
        public object[] ResultMemberStressesAtStart(int memberID) { return (object[])CallMethod(App, "StressesAtStartOfMember", new object[] { memberID }); }
        public object[] ResultMemberStressesAtEnd(int memberID) { return (object[])CallMethod(App, "StressesAtEndOfMember", new object[] { memberID }); }
        public object[] ResultMemberStressesInMid(int memberID) { return (object[])CallMethod(App, "StressesInMidOfMember", new object[] { memberID }); }
        public object[] ResultMemberStressesAtStation(int memberID, double Station) { return (object[])CallMethod(App, "StressesAtStationAlongMember", new object[] { memberID, Station }); }
        public object[] ResultPlateForcesAtJoint(int plateID, int joint) { return (object[])CallMethod(App, "ForcesAtJointOfPlate", new object[] { plateID, joint }); }
        public object[] ResultPlateForcesAtJointInt(int plateID, int joint) { return (object[])CallMethod(App, "ForcesAtJointOfPlateInt", new object[] { plateID, joint }); }
        public object[] ResultPlateForcesAtCenter(int plateID) { return (object[])CallMethod(App, "ForcesAtCenterOfPlate", new object[] { plateID }); }
        public object[] ResultPlateStressesAtTop(int plateID) { return (object[])CallMethod(App, "StressesAtTopOfPlate", new object[] { plateID }); }
        public object[] ResultPlateStressesInMid(int plateID) { return (object[])CallMethod(App, "StressesInMidOfPlate", new object[] { plateID }); }
        public object[] ResultPlateStressesInBottom(int plateID) { return (object[])CallMethod(App, "StressesAtBottomOfPlate", new object[] { plateID }); }

        public int ResultEnvCol { get { return (int)GetProp(App, "ResultEnvelopeColumn", new object[] { }); } set { SetProp(App, "ResultEnvelopeColumn", value); } }
        public bool ResultEnvAbs { get { return (bool)GetProp(App, "ResultEnvelopeABS", new object[] { }); } set { SetProp(App, "ResultEnvelopeABS", value); } }
        public bool ResultEnvMin { get { return (bool)GetProp(App, "ResultLoadMin", new object[] { }); } set { SetProp(App, "ResultLoadMin", value); } }
        public bool ResultInUCS { get { return (bool)GetProp(App, "ResultInUCS", new object[] { }); } set { SetProp(App, "ResultInUCS", value); } }
        public bool ResultIncremental { get { return (bool)GetProp(App, "ResultIncremental", new object[] { }); } set { SetProp(App, "ResultIncremental", value); } }
        public int ResultLoadClass { get { return (int)GetProp(App, "ResultLoadClass", new object[] { }); } set { SetProp(App, "ResultLoadClass", value); } }

        public LarsaSection GetSectionByName(string name) { return GetObj<LarsaSection>(CallMethod(App, "GetSectionByName", new object[] { name, true })); }
        public LarsaMaterial GetMaterialByName(string name) { return GetObj<LarsaMaterial>(CallMethod(App, "GetMaterialByName", new object[] { name, true })); }
        public LarsaUCS GetUCSByName(string name) { return GetObj<LarsaUCS>(CallMethod(App, "GetUCSByName", new object[] { name, true })); }
        public LarsaLoadCase GetLoadCaseByName(string name) { return GetObj<LarsaLoadCase>(CallMethod(App, "GetLoadCaseByName", new object[] { name, true })); }
        public LarsaLoadCombination GetLoadCombinationByName(string name) { return GetObj<LarsaLoadCombination>(CallMethod(App, "GetLoadCombinationByName", new object[] { name, true })); }

        public void ModifyBreakMemberToExitingJoints(LarsaMember m) { CallMethod(App, "GENBreakMemberToExitingJoints", new object[] { m.o }); }
        public void ModifyBreakIntersectingMembers() { CallMethod(App, "GENBreakIntersectingMembers", new object[] { }); }
        public void ModifyBreakPlatesIntersectingWithMembers() { CallMethod(App, "GENBreakPlatesIntersectingWithMembers", new object[] { }); }
        public void ModifyBreakPlateToJoints(LarsaPlate p) { CallMethod(App, "GENBreakPlateToJoints", new object[] { p.o }); }
        public void ModifyGeneratePlatesFromExistingMembers(bool TriangeOnly) { CallMethod(App, "GeneratePlatesFromExistingMembers", new object[] { TriangeOnly }); }

        public struct MergeJoint { public int JointID; public double X; public double Y; public double Z; public List<MergeJoint> Group;}
        public List<List<MergeJoint>> ModifyGetJointsAtSameLocation(double tolerance) 
        {
            List<MergeJoint> joints = new List<MergeJoint>();
            foreach (LarsaJoint j in Joints) joints.Add(new MergeJoint() { JointID = j.Number, X = j.X, Y = j.Y, Z = j.Z });

            joints = ModifyMergeSort(joints, delegate(MergeJoint p1, MergeJoint p2) { return p1.Z < p2.Z; });
            joints = ModifyMergeSort(joints, delegate(MergeJoint p1, MergeJoint p2) { return p1.Y < p2.Y; });
            joints = ModifyMergeSort(joints, delegate(MergeJoint p1, MergeJoint p2) { return p1.X < p2.X; });
            
            List<List<MergeJoint>> Groups = new List<List<MergeJoint>>();

            for (int i = 0; i < joints.Count - 1; i++)
            {
                MergeJoint joint1 = joints[i];
                MergeJoint joint2 = joints[i + 1];
                if (Math.Abs(joint1.X - joint2.X) < tolerance)
                    if (Math.Abs(joint1.Y - joint2.Y) < tolerance)
                        if (Math.Abs(joint1.Z - joint2.Z) < tolerance)
                        {
                            if (joint1.Group != null)
                            {
                                joint1.Group.Add(joint2);
                                joint2.Group = joint1.Group;
                                continue;
                            }

                            joint1.Group = new List<MergeJoint>();
                            joint1.Group.Add(joint1);
                            joint1.Group.Add(joint2);
                            joint2.Group = joint1.Group;
                            Groups.Add(joint1.Group);
                        }
            }
            return Groups;
        }
        public int ModifyMergeJoints(double tolerance) 
        {
            List<List<MergeJoint>> Groups = ModifyGetJointsAtSameLocation(tolerance);
            int count = 0;
            foreach (List<MergeJoint> group in Groups)
            {
                MergeJoint j1 = group[0];
                LarsaJoint j1o = this.Joints.ItemByKey(j1.JointID);

                for (int i = 1; i < group.Count; i++)
                {
                    MergeJoint j2 = group[i];
                    LarsaJoint j2o = this.Joints.ItemByKey(j2.JointID);
                    foreach (LarsaMember m in Members)
                    {
                        if (m.IJoint != null && m.IJoint.Number == j2.JointID) m.IJoint = j1o;
                        if (m.JJoint != null && m.JJoint.Number == j2.JointID) m.JJoint = j1o;
                    }

                    foreach (LarsaPlate pl in Plates)
                    {
                        if (pl.IJoint != null && pl.IJoint.Number == j2.JointID) pl.IJoint = j1o;
                        if (pl.JJoint != null && pl.JJoint.Number == j2.JointID) pl.JJoint = j1o;
                        if (pl.KJoint != null && pl.KJoint.Number == j2.JointID) pl.JJoint = j1o;
                        if (pl.LJoint != null && pl.LJoint.Number == j2.JointID) pl.JJoint = j1o;
                    }

                    foreach (LarsaSpring sp in Springs)
                    {
                        if (sp.IJoint != null && sp.IJoint.Number == j2.JointID) sp.IJoint = j1o;
                        if (sp.JJoint != null && sp.JJoint.Number == j2.JointID) sp.JJoint = j1o;
                    }

                    Joints.Remove(j2o); count++;
                }
            }
            
            return count;
        }

        public delegate bool MergeSortIsP1LessThanP2<T>(T p1, T p2);
        public List<T> ModifyMergeSort<T>(List<T> a, MergeSortIsP1LessThanP2<T> comp)
        {
            if (a.Count == 1) return a;
            List<T> sorted = new List<T>();
            int middle = (int)a.Count / 2;
            List<T> leftArray = a.GetRange(0, middle);
            List<T> rightArray = a.GetRange(middle, a.Count - middle);
            leftArray = ModifyMergeSort(leftArray, comp);
            rightArray = ModifyMergeSort(rightArray, comp);
            int leftptr = 0;
            int rightptr = 0;
            for (int i = 0; i < leftArray.Count + rightArray.Count; i++)
            {
                if (leftptr == leftArray.Count)
                {
                    sorted.Add(rightArray[rightptr]);
                    rightptr++;
                }
                else if (rightptr == rightArray.Count)
                {
                    sorted.Add(leftArray[leftptr]);
                    leftptr++;
                }
                else if (comp(leftArray[leftptr], rightArray[rightptr]))
                {
                    //need the cast above since arraylist returns Type object
                    sorted.Add(leftArray[leftptr]);
                    leftptr++;
                }
                else
                {
                    sorted.Add(rightArray[rightptr]);
                    rightptr++;
                }
            }
            return sorted;
        }



        public static object CreateObject(string lib, string obj)
        {
            if (App == null) { Type AppType = Type.GetTypeFromProgID(lib + "." + obj); return Activator.CreateInstance(AppType); }
            else { return CallMethod(App, "CreateObject", new object[] { lib, obj }); }
        }

        public static T GetObj<T>(object o)
        {
            if (o == null) return default(T);
            MethodInfo method = typeof(LarsaApp).GetMethod("GetObj");
            return (T)Activator.CreateInstance(typeof(T), new object[] { o });
        }
        public static void SetObj(object o, string method, object value)
        {
            object svalue = null;
            if (value != null) svalue = value.GetType().GetField("o").GetValue(value);
            o.GetType().GetMethod(method).Invoke(o, new object[] { svalue });
        }
        public static object GetNativeObj(object o)
        {
            object svalue = null;
            if (o != null) svalue = o.GetType().GetField("o").GetValue(o);
            return svalue;
        }
        public static object CallMethod(object o, string method)
        {
            return o.GetType().InvokeMember(method, BindingFlags.InvokeMethod, null, o, new object[] { });
        }
        public static object CallMethod(object o, string method, object arg)
        {
            return o.GetType().InvokeMember(method, BindingFlags.InvokeMethod, null, o, new object[] { arg });
        }
        public static object CallMethod(object o, string method, object arg1, object arg2)
        {
            return o.GetType().InvokeMember(method, BindingFlags.InvokeMethod, null, o, new object[] { arg1, arg2 });
        }
        public static object CallMethod(object o, string method, object arg1, object arg2, object arg3)
        {
            return o.GetType().InvokeMember(method, BindingFlags.InvokeMethod, null, o, new object[] { arg1, arg2, arg3 });
        }
        public static object CallMethod(object o, string method, object arg1, object arg2, object arg3, object arg4)
        {
            return o.GetType().InvokeMember(method, BindingFlags.InvokeMethod, null, o, new object[] { arg1, arg2, arg3, arg4 });
        }
        public static object CallMethod(object o, string method, object arg1, object arg2, object arg3, object arg4, object arg5)
        {
            return o.GetType().InvokeMember(method, BindingFlags.InvokeMethod, null, o, new object[] { arg1, arg2, arg3, arg4, arg5 });
        }
        public static object CallMethod(object o, string method, object[] args)
        {
            return o.GetType().InvokeMember(method, BindingFlags.InvokeMethod, null, o, args);
        }
        public static object SetProp(object o, string prop, object arg)
        {
            return o.GetType().InvokeMember(prop, BindingFlags.SetProperty, null, o, new object[] { arg });
        }
        public static object SetProp(object o, string prop, object arg1, object arg2)
        {
            return o.GetType().InvokeMember(prop, BindingFlags.SetProperty, null, o, new object[] { arg1, arg2 });
        }
        public static object SetProp(object o, string prop, object[] args)
        {
            return o.GetType().InvokeMember(prop, BindingFlags.SetProperty, null, o, args);
        }
        public static object GetProp(object o, string prop, object arg)
        {
            return o.GetType().InvokeMember(prop, BindingFlags.GetProperty, null, o, new object[] { arg });

        }
        public static object GetProp(object o, string prop, object arg1, object arg2)
        {
            return o.GetType().InvokeMember(prop, BindingFlags.GetProperty, null, o, new object[] { arg1, arg2 });
        }
        public static object GetProp(object o, string prop)
        {
            return o.GetType().InvokeMember(prop, BindingFlags.GetProperty, null, o, new object[] { });
        }
        public static object GetProp(object o, string prop, object[] args)
        {
            return o.GetType().InvokeMember(prop, BindingFlags.GetProperty, null, o, args);
        }
    }

    public class LarsaArray<T> : IList<T>, IEnumerator<T>
    {
        private object a;
        public LarsaArray(object array) { a = array; }
        public int Count { get { return (int)LarsaApp.GetProp(a, "count"); } }

        public int IndexOf(T item) { return (int)LarsaApp.GetProp(a, "indexOf", LarsaApp.GetNativeObj(item)); }

        public void Add(T item)
        {
            object o = LarsaApp.GetNativeObj(item);
            int key = (int)LarsaApp.GetProp(o, "number");
            if (key < 0) key = 0;
            LarsaApp.CallMethod(a, "Append", o, key);
        }

        public void Insert(int index, T item)
        {
            index++;
            object o = LarsaApp.GetNativeObj(item);
            int key = (int)LarsaApp.GetProp(o, "number");
            LarsaApp.CallMethod(a, "insert", o, index, key);
        }

        public void RemoveAt(int index) { index++; LarsaApp.CallMethod(a, "removeRange", index, index); }

        public T this[int index]
        {
            get { index++; return LarsaApp.GetObj<T>(LarsaApp.CallMethod(a, "itemByIndex", index)); }
            set { throw new Exception("You can not set this list directly, please use Add or Remove methods instead."); }
        }

        public void Clear() { LarsaApp.CallMethod(a, "Clear"); }

        public bool Contains(T item)
        {
            object o = LarsaApp.GetNativeObj(item);
            return (bool)LarsaApp.CallMethod(a, "contains", o);
        }

        public bool Remove(T item)
        {
            object o = LarsaApp.GetNativeObj(item);
            int key = (int)LarsaApp.GetProp(o, "number");
            LarsaApp.CallMethod(a, "Remove", key);
            return true;
        }

        public T ItemByKey(int number) { return LarsaApp.GetObj<T>(LarsaApp.CallMethod(a, "itemByKey",number)); }

        public void CopyTo(T[] array, int arrayIndex) { throw new NotImplementedException(); }
        public bool IsReadOnly { get { throw new NotImplementedException(); } }

        public IEnumerator<T> GetEnumerator() { return this; }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return this; }
        private int EnumIndex = -1;
        public T Current { get { return this[EnumIndex]; } }
        object System.Collections.IEnumerator.Current { get { return this[EnumIndex]; } }
        public bool MoveNext() { EnumIndex++; bool ret = (EnumIndex < this.Count); if (!ret) Reset(); return ret; }
        public void Reset() { EnumIndex = -1; }
        public void Dispose() { }
    }

    public enum Length { None = -1, Meter = 0, Centimeter = 1, Millimeter = 2, Feet = 3, Inch = 4, Yard = 5 }
    public enum Force { None = -1, MegaNewton = 0, KiloNewton = 1, Newton = 2, Kip = 3, Pound = 4, Ounce = 5, Ton = 6, MetricTon = 7, KilogramForce = 8 }
    public enum Temperature { None = -1, Celsius = 0, Fahrenheit = 1, Centigrade = 2, Rankin = 3, Kelvin = 4 }
    public enum Angle { None = -1, Degree = 0, Radian = 1 }
    public class LarsaUnits
    {
        public object o;
        public LarsaUnits() { o = LarsaApp.CreateObject("LarsaElements", "clsUnits"); }
        public LarsaUnits(object obj) { o = obj; }
        public Length CoordinateUnit { get { return (Length)LarsaApp.CallMethod(o, "GetUnit", 3, 0); } set { LarsaApp.CallMethod(o, "SetUnits", 3, true, value); } }
        public Length SectionLengthUnit { get { return (Length)LarsaApp.CallMethod(o, "GetUnit", 1, 0); } set { LarsaApp.CallMethod(o, "SetUnits", 1, true, value); } }
        public Length MaterialLengthUnit { get { return (Length)LarsaApp.CallMethod(o, "GetUnit", 0, 0); } set { LarsaApp.CallMethod(o, "SetUnits", 0, true, value); } }
        public Force MaterialForceUnit { get { return (Force)LarsaApp.CallMethod(o, "GetUnit", 0, 1); } set { LarsaApp.CallMethod(o, "SetUnits", 0, true, -1, value); } }
        public Temperature MaterialTemperatureUnit { get { return (Temperature)LarsaApp.CallMethod(o, "GetUnit", 0, 2); } set { LarsaApp.CallMethod(o, "SetUnits", 0, true, -1, -1, value); } }
        public Length LoadsLengthUnit { get { return (Length)LarsaApp.CallMethod(o, "GetUnit", 4, 0); } set { LarsaApp.CallMethod(o, "SetUnits", 4, true, value); } }
        public Force LoadsForceUnit { get { return (Force)LarsaApp.CallMethod(o, "GetUnit", 4, 1); } set { LarsaApp.CallMethod(o, "SetUnits", 4, true, -1, value); } }
        public Temperature LoadsTemperatureUnit { get { return (Temperature)LarsaApp.CallMethod(o, "GetUnit", 4, 2); } set { LarsaApp.CallMethod(o, "SetUnits", 4, true, -1, -1, value); } }
        public Length SpringLengthUnit { get { return (Length)LarsaApp.CallMethod(o, "GetUnit", 2, 0); } set { LarsaApp.CallMethod(o, "SetUnits", 2, true, value); } }
        public Force SpringForceUnit { get { return (Force)LarsaApp.CallMethod(o, "GetUnit", 2, 1); } set { LarsaApp.CallMethod(o, "SetUnits", 2, true, -1, value); } }
        public Length MassLengthUnit { get { return (Length)LarsaApp.CallMethod(o, "GetUnit", 5, 0); } set { LarsaApp.CallMethod(o, "SetUnits", 5, true, value); } }
        public Force MassForceUnit { get { return (Force)LarsaApp.CallMethod(o, "GetUnit", 5, 1); } set { LarsaApp.CallMethod(o, "SetUnits", 5, true, -1, value); } }
        public void SetUnitsTo(Force ForceUnit) { SetUnitsTo(Length.None, ForceUnit, Temperature.None); }
        public void SetUnitsTo(Length LengthUnit) { SetUnitsTo(LengthUnit, Force.None, Temperature.None); }
        public void SetUnitsTo(Temperature TempUnit) { SetUnitsTo(Length.None, Force.None, TempUnit); }
        public void SetUnitsTo(Length LengthUnit, Force ForceUnit) { SetUnitsTo(LengthUnit, ForceUnit, Temperature.None); }
        public void SetUnitsTo(Length LengthUnit, Temperature TempUnit) { SetUnitsTo(LengthUnit, Force.None, TempUnit); }
        public void SetUnitsTo(Force ForceUnit, Temperature TempUnit) { SetUnitsTo(Length.None, ForceUnit, TempUnit); }
        public void SetUnitsTo(Length LengthUnit, Force ForceUnit, Temperature TempUnit)
        {
            if (LengthUnit != Length.None)
            {
                this.CoordinateUnit = LengthUnit;
                this.SectionLengthUnit = LengthUnit;
                this.MaterialLengthUnit = LengthUnit;
                this.LoadsLengthUnit = LengthUnit;
                this.SpringLengthUnit = LengthUnit;
                this.MassLengthUnit = LengthUnit;
            }
            if (ForceUnit != Force.None)
            {
                this.MaterialForceUnit = ForceUnit;
                this.LoadsForceUnit = ForceUnit;
                this.SpringForceUnit = ForceUnit;
                this.MassForceUnit = ForceUnit;
            }
            if (TempUnit != Temperature.None)
            {
                this.MaterialTemperatureUnit = TempUnit;
                this.LoadsTemperatureUnit = TempUnit;
            }
        }
    }

    public class LarsaJoint
    {
        public object o;
        public LarsaJoint() { o = LarsaApp.CreateObject("LarsaElements", "clsJoint"); }
        public LarsaJoint(object obj) { o = obj; }
        public int Number { get { return (int)LarsaApp.GetProp(o, "number"); } set { LarsaApp.SetProp(o, "number", value); } }
        public double X { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "location"), "x"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "location"), "x", value); } }
        public double Y { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "location"), "y"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "location"), "y", value); } }
        public double Z { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "location"), "z"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "location"), "z", value); } }
        public void SetCoordinates(double x, double y, double z) { X = x; Y = y; Z = z; }
        public string Constraint { get { return (string)LarsaApp.GetProp(o, "Constraint"); } set { LarsaApp.SetProp(o, "Constraint", value); } }
        public LarsaUCS DispUCS { get { return LarsaApp.GetObj<LarsaUCS>(LarsaApp.GetProp(o, "displacementUCS")); } set { LarsaApp.SetProp(o, "displacementUCS", LarsaApp.GetNativeObj(value)); } }
    }

    public class LarsaMember
    {
        public object o;
        public LarsaMember() { o = LarsaApp.CreateObject("LarsaElements", "clsMember"); }
        public LarsaMember(object obj) { o = obj; }
        public int Number { get { return (int)LarsaApp.GetProp(o, "number"); } set { LarsaApp.SetProp(o, "number", value); } }
        public LarsaJoint IJoint { get { return LarsaApp.GetObj<LarsaJoint>(LarsaApp.GetProp(o, "joint", 1)); } set { LarsaApp.SetProp(o, "joint", 1, LarsaApp.GetNativeObj(value)); } }
        public LarsaJoint JJoint { get { return LarsaApp.GetObj<LarsaJoint>(LarsaApp.GetProp(o, "joint", 2)); } set { LarsaApp.SetProp(o, "joint", 2, LarsaApp.GetNativeObj(value)); } }
        public LarsaMaterial Material { get { return LarsaApp.GetObj<LarsaMaterial>(LarsaApp.GetProp(o, "material")); } set { LarsaApp.SetProp(o, "material", LarsaApp.GetNativeObj(value)); } }
        public LarsaSection Section1 { get { return LarsaApp.GetObj<LarsaSection>(LarsaApp.GetProp(o, "section", 1)); } set { LarsaApp.SetProp(o, "section", 1, LarsaApp.GetNativeObj(value)); } }
        public LarsaSection Section2 { get { return LarsaApp.GetObj<LarsaSection>(LarsaApp.GetProp(o, "section", 2)); } set { LarsaApp.SetProp(o, "section", 2, LarsaApp.GetNativeObj(value)); } }
        public float OrientationAngle { get { return (float)LarsaApp.GetProp(o, "orientationAngle"); } set { LarsaApp.SetProp(o, "orientationAngle", value); } }
        public double PrestressForce { get { return (double)LarsaApp.GetProp(o, "prestressForce"); } set { LarsaApp.SetProp(o, "prestressForce", value); } }
    }

    public class LarsaMemberFixity
    {
        public object o;
        public LarsaMemberFixity() { o = LarsaApp.CreateObject("LarsaElements", "clsMemberFixity"); }
        public LarsaMemberFixity(object obj) { o = obj; }
        public int Number { get { return (int)LarsaApp.GetProp(o, "number"); } set { LarsaApp.SetProp(o, "number", value); } }
        public LarsaMember Member { get { return LarsaApp.GetObj<LarsaMember>(LarsaApp.GetProp(o, "member", 1)); } set { LarsaApp.SetProp(o, "member", LarsaApp.GetNativeObj(value)); } }
        public int MemberID { get { return (int)LarsaApp.GetProp(o, "memberID"); } }
        
        // a value from 0 to 100 should be specified. 0 means released, 100 means fixed.
        public double StartFX { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "fixity", 1, 1), "x"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "fixity", 1, 1), "x", value); } }
        public double StartFY { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "fixity", 1, 1), "y"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "fixity", 1, 1), "y", value); } }
        public double StartFZ { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "fixity", 1, 1), "z"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "fixity", 1, 1), "z", value); } }
        public double StartMX { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "fixity", 1, 2), "x"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "fixity", 1, 2), "x", value); } }
        public double StartMY { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "fixity", 1, 2), "y"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "fixity", 1, 2), "y", value); } }
        public double StartMZ { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "fixity", 1, 2), "z"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "fixity", 1, 2), "z", value); } }

        public double EndFX { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "fixity", 2, 1), "x"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "fixity", 2, 1), "x", value); } }
        public double EndFY { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "fixity", 2, 1), "y"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "fixity", 2, 1), "y", value); } }
        public double EndFZ { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "fixity", 2, 1), "z"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "fixity", 2, 1), "z", value); } }
        public double EndMX { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "fixity", 2, 2), "x"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "fixity", 2, 2), "x", value); } }
        public double EndMY { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "fixity", 2, 2), "y"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "fixity", 2, 2), "y", value); } }
        public double EndMZ { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "fixity", 2, 2), "z"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "fixity", 2, 2), "z", value); } }
    }

    public class LarsaMaterial
    {
        public object o;
        public LarsaMaterial() { o = (object)LarsaApp.CreateObject("LarsaElements", "clsMaterial"); }
        public LarsaMaterial(object obj) { o = obj; }
        public int Number { get { return (int)LarsaApp.GetProp(o, "number"); } set { LarsaApp.SetProp(o, "number", value); } }
        public string Name { get { return (string)LarsaApp.GetProp(o, "name"); } set { LarsaApp.SetProp(o, "name", value); } }
        public double E { get { return (double)LarsaApp.GetProp(o, "modulusOfElasticity"); } set { LarsaApp.SetProp(o, "modulusOfElasticity", value); } }
        public double G { get { return (double)LarsaApp.GetProp(o, "shearModulus"); } set { LarsaApp.SetProp(o, "shearModulus", value); } }
        public double UnitWeight { get { return (double)LarsaApp.GetProp(o, "unitWeight"); } set { LarsaApp.SetProp(o, "unitWeight", value); } }
        public double ThermalExpansion { get { return (double)LarsaApp.GetProp(o, "coefficientOfThermalExpansion"); } set { LarsaApp.SetProp(o, "coefficientOfThermalExpansion", value); } }
        public double Fy { get { return (double)LarsaApp.GetProp(o, "YieldStress"); } set { LarsaApp.SetProp(o, "YieldStress", value); } }
        public double Fu { get { return (double)LarsaApp.GetProp(o, "concreteFc28"); } set { LarsaApp.SetProp(o, "concreteFc28", value); } }
        public double TendonGUTS { get { return (double)LarsaApp.GetProp(o, "tendonGUTS"); } set { LarsaApp.SetProp(o, "tendonGUTS", value); } }
    }

    public class LarsaSection
    {
        public object o;
        public LarsaSection() { o = LarsaApp.CreateObject("LarsaElements", "clsSection"); }
        public LarsaSection(object obj) { o = obj; }
        public int Number { get { return (int)LarsaApp.GetProp(o, "number"); } set { LarsaApp.SetProp(o, "number", value); } }
        public string Name { get { return (string)LarsaApp.GetProp(o, "name"); } set { LarsaApp.SetProp(o, "name", value); } }
        public double Area { get { return (double)LarsaApp.GetProp(o, "sectionArea"); } set { LarsaApp.SetProp(o, "sectionArea", value); } }
        public double ShearAreaY { get { return (double)LarsaApp.GetProp(o, "shearAreaY"); } set { LarsaApp.SetProp(o, "shearAreaY", value); } }
        public double ShearAreaZ { get { return (double)LarsaApp.GetProp(o, "shearAreaZ"); } set { LarsaApp.SetProp(o, "shearAreaZ", value); } }
        public double J { get { return (double)LarsaApp.GetProp(o, "torsionj"); } set { LarsaApp.SetProp(o, "torsionj", value); } }
        public double Iy { get { return (double)LarsaApp.GetProp(o, "inertiaY"); } set { LarsaApp.SetProp(o, "inertiaY", value); } }
        public double Iz { get { return (double)LarsaApp.GetProp(o, "inertiaZ"); } set { LarsaApp.SetProp(o, "inertiaZ", value); } }
        public void SetAsRectangleSection(double depth, double width) { LarsaApp.CallMethod(o, "forceShapeValues", 1, depth, width); LarsaApp.CallMethod(o, "doCalculations"); }
        public void SetAsCircularSection(double diameter) { LarsaApp.CallMethod(o, "forceShapeValues", 2, diameter); LarsaApp.CallMethod(o, "doCalculations"); }
        public void SetAsISection(double depth, double flangewidth, double flangethickness, double webthickness) { LarsaApp.CallMethod(o, "forceShapeValues", 3, depth, flangewidth, flangethickness, webthickness); LarsaApp.CallMethod(o, "doCalculations"); }
        public void SetAsLSection(double depth, double width, double thickness) { LarsaApp.CallMethod(o, "forceShapeValues", 4, depth, width, thickness); LarsaApp.CallMethod(o, "doCalculations"); }
        public void SetAsTSection(double depth, double flangewidth, double flangethickness, double webthickness) { LarsaApp.CallMethod(o, "forceShapeValues", 5, depth, flangewidth, webthickness, flangethickness); LarsaApp.CallMethod(o, "doCalculations"); }
        public void SetAsCSection(double depth, double flangewidth, double flangethickness, double webthickness) { LarsaApp.CallMethod(o, "forceShapeValues", 6, depth, flangewidth, webthickness, flangethickness); LarsaApp.CallMethod(o, "doCalculations"); }
        public void SetAsPipeSection(double diameter, double thickness) { LarsaApp.CallMethod(o, "forceShapeValues", 7, diameter, thickness); LarsaApp.CallMethod(o, "doCalculations"); }
        public void SetAsTubeSection(double depth, double width, double thickness) { LarsaApp.CallMethod(o, "forceShapeValues", 8, depth, width, thickness); LarsaApp.CallMethod(o, "doCalculations"); }
        public void SetAsTriangleSection(double depth, double width) { LarsaApp.CallMethod(o, "forceShapeValues", 10, depth, width); LarsaApp.CallMethod(o, "doCalculations"); }
        public void SetAsTrapezoidSection(double depth, double width1, double width2) { LarsaApp.CallMethod(o, "forceShapeValues", 11, depth, width1, width2); LarsaApp.CallMethod(o, "doCalculations"); }
        public void SetAsHalfCircleSection(double diameter) { LarsaApp.CallMethod(o, "forceShapeValues", 12, diameter); LarsaApp.CallMethod(o, "doCalculations"); }
        public void SetAsZSection(double depth, double flangewidth, double thickness) { LarsaApp.CallMethod(o, "forceShapeValues", 13, depth, flangewidth, thickness); LarsaApp.CallMethod(o, "doCalculations"); }
    }

    public class LarsaPlate
    {
        public object o;
        public LarsaPlate() { o = LarsaApp.CreateObject("LarsaElements", "clsPlate"); }
        public LarsaPlate(object obj) { o = obj; }
        public int Number { get { return (int)LarsaApp.GetProp(o, "number"); } set { LarsaApp.SetProp(o, "number", value); } }
        public LarsaJoint IJoint { get { return LarsaApp.GetObj<LarsaJoint>(LarsaApp.GetProp(o, "joint", 1)); } set { LarsaApp.SetProp(o, "joint", 1, LarsaApp.GetNativeObj(value)); } }
        public LarsaJoint JJoint { get { return LarsaApp.GetObj<LarsaJoint>(LarsaApp.GetProp(o, "joint", 2)); } set { LarsaApp.SetProp(o, "joint", 2, LarsaApp.GetNativeObj(value)); } }
        public LarsaJoint KJoint { get { return LarsaApp.GetObj<LarsaJoint>(LarsaApp.GetProp(o, "joint", 3)); } set { LarsaApp.SetProp(o, "joint", 3, LarsaApp.GetNativeObj(value)); } }
        public LarsaJoint LJoint { get { return LarsaApp.GetObj<LarsaJoint>(LarsaApp.GetProp(o, "joint", 4)); } set { LarsaApp.SetProp(o, "joint", 4, LarsaApp.GetNativeObj(value)); } }
        public LarsaMaterial Material { get { return LarsaApp.GetObj<LarsaMaterial>(LarsaApp.GetProp(o, "material")); } set { LarsaApp.SetProp(o, "material", LarsaApp.GetNativeObj(value)); } }
        public double Thickness { get { return (double)LarsaApp.GetProp(o, "thickness"); } set { LarsaApp.SetProp(o, "thickness", value); } }

    }

    public enum LarsaSpringTypes { Inactive = 0, Linear = 1, Nonlinear = 2, Hysteretic = 3 }
    public enum LarsaSpringDirections { TX = 0, TY = 1, TZ = 2, RX = 3, RY = 4, RZ = 5, LocalAxial = 6, LocalTorsion = 7 }
    public class LarsaSpring
    {
        public object o;
        public LarsaSpring() { o = LarsaApp.CreateObject("LarsaElements", "clsSpring"); }
        public LarsaSpring(object obj) { o = obj; }
        public int Number { get { return (int)LarsaApp.GetProp(o, "number"); } set { LarsaApp.SetProp(o, "number", value); } }
        public LarsaJoint IJoint { get { return LarsaApp.GetObj<LarsaJoint>(LarsaApp.GetProp(o, "joint", 1)); } set { LarsaApp.SetProp(o, "joint", 1, LarsaApp.GetNativeObj(value)); } }
        public LarsaJoint JJoint { get { return LarsaApp.GetObj<LarsaJoint>(LarsaApp.GetProp(o, "joint", 2)); } set { LarsaApp.SetProp(o, "joint", 2, LarsaApp.GetNativeObj(value)); } }
        public LarsaSpringTypes Type { get { return (LarsaSpringTypes)LarsaApp.GetProp(o, "springType"); } set { LarsaApp.SetProp(o, "springType", value); } }
        public LarsaSpringDirections Direction { get { return (LarsaSpringDirections)LarsaApp.GetProp(o, "direction"); } set { LarsaApp.SetProp(o, "direction", value); } }
        public double KTension { get { return (double)LarsaApp.GetProp(o, "kTension"); } set { LarsaApp.SetProp(o, "kTension", value); } }
        public double KCompression { get { return (double)LarsaApp.GetProp(o, "kCompression"); } set { LarsaApp.SetProp(o, "kCompression", value); } }
        public double MaxTension { get { return (double)LarsaApp.GetProp(o, "maxTension"); } set { LarsaApp.SetProp(o, "maxTension", value); } }
        public double MaxCompression { get { return (double)LarsaApp.GetProp(o, "maxCompression"); } set { LarsaApp.SetProp(o, "maxCompression", value); } }
        public double Hook { get { return (double)LarsaApp.GetProp(o, "hook"); } set { LarsaApp.SetProp(o, "hook", value); } }
        public double Gap { get { return (double)LarsaApp.GetProp(o, "gap"); } set { LarsaApp.SetProp(o, "gap", value); } }
        public double Damping { get { return (double)LarsaApp.GetProp(o, "damping"); } set { LarsaApp.SetProp(o, "damping", value); } }
    }

    public enum LarsaUCSType { Cartesian = 0, Cylindrical = 1, Spherical = 2, BridgePath = 3 }
    public class LarsaUCS
    {
        public object o;
        public LarsaUCS() { o = LarsaApp.CreateObject("LarsaElements", "clsUserCoorSystem"); }
        public LarsaUCS(object obj) { o = obj; }
        public int Number { get { return (int)LarsaApp.GetProp(o, "number"); } set { LarsaApp.SetProp(o, "number", value); } }
        public string Name { get { return (string)LarsaApp.GetProp(o, "name"); } set { LarsaApp.SetProp(o, "name", value); } }
        public LarsaUCSType Type { get { return (LarsaUCSType)LarsaApp.GetProp(o, "ucsType"); } set { LarsaApp.SetProp(o, "ucsType", value); } }
        public LarsaBridgePath BridgePath { get { return new LarsaBridgePath(LarsaApp.GetProp(o, "path")); } }
        public double OriginX { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "point", 1), "x"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "point", 1), "x", value); } }
        public double OriginY { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "point", 1), "y"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "point", 1), "y", value); } }
        public double OriginZ { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "point", 1), "z"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "point", 1), "z", value); } }
    }
    public class LarsaBridgePath
    {
        public object o;
        public LarsaBridgePath() { o = LarsaApp.CreateObject("LarsaElements", "clsRichPath"); }
        public LarsaBridgePath(object obj) { o = obj; }
        public int Number { get { return (int)LarsaApp.GetProp(o, "number"); } set { LarsaApp.SetProp(o, "number", value); } }
        public void AddPoint() { LarsaApp.CallMethod(o, "AddPoint"); }
        public void AddElevation() { LarsaApp.CallMethod(o, "AddElevation"); }
        public LarsaBridgePathPoint Point(int index) { return new LarsaBridgePathPoint(LarsaApp.GetProp(o, "point", index)); }
        public LarsaBridgePathCurve Curve(int index) { return new LarsaBridgePathCurve(LarsaApp.GetProp(o, "Curve", index)); }
        public LarsaBridgePathElevation Elevation(int index) { return new LarsaBridgePathElevation(LarsaApp.GetProp(o, "Elevation", index)); }
    }
    public enum LarsaBridgePathCurveType { Circular = 0, BigCircular = 1, Straight = 2, SpiralClockwise = 3, SpiralCounterClockwise = 4, SpiralClockwiseInf = 5, SpiralCounterClockwiseInf = 6 }
    public class LarsaBridgePathPoint
    {
        public object o;
        public LarsaBridgePathPoint() { o = LarsaApp.CreateObject("LarsaElements", "clsRichPathPoint"); }
        public LarsaBridgePathPoint(object obj) { o = obj; }
        public double ArcPosition { get { return (double)LarsaApp.GetProp(o, "ArcPosition"); } set { LarsaApp.SetProp(o, "ArcPosition", value); } }
    }
    public class LarsaBridgePathCurve
    {
        public object o;
        public LarsaBridgePathCurve() { o = LarsaApp.CreateObject("LarsaElements", "clsRichPathCurve"); }
        public LarsaBridgePathCurve(object obj) { o = obj; }
        public LarsaBridgePathCurveType CurveType { get { return (LarsaBridgePathCurveType)LarsaApp.GetProp(o, "curveType"); } set { LarsaApp.SetProp(o, "curveType", value); } }
        public double Radius { get { return (double)LarsaApp.GetProp(o, "Radius"); } set { LarsaApp.SetProp(o, "Radius", value); } }
    }
    public class LarsaBridgePathElevation
    {
        public object o;
        public LarsaBridgePathElevation() { o = LarsaApp.CreateObject("LarsaElements", "clsRichPathElevation"); }
        public LarsaBridgePathElevation(object obj) { o = obj; }
        public double ArcPosition { get { return (double)LarsaApp.GetProp(o, "ArcPosition"); } set { LarsaApp.SetProp(o, "ArcPosition", value); } }
        public double Elevation { get { return (double)LarsaApp.GetProp(o, "Elevation"); } set { LarsaApp.SetProp(o, "Elevation", value); } }
        public double Grade { get { return (double)LarsaApp.GetProp(o, "Grade"); } set { LarsaApp.SetProp(o, "Grade", value); } }
    }


    public class LarsaLane
    {
        public object o;
        public LarsaLane() { o = LarsaApp.CreateObject("LarsaElements", "clsLane"); }
        public LarsaLane(object obj) { o = obj; }

        public long laneIndex { get { return (long)LarsaApp.GetProp(o, "number"); } set { LarsaApp.SetProp(o, "number", value); } }

        public string laneName { get { return (string)LarsaApp.GetProp(o, "Name"); } set { LarsaApp.SetProp(o, "Name", value); } }

        public double laneWidth { get { return (double)LarsaApp.GetProp(o, "width"); } set { LarsaApp.SetProp(o, "width", value); } }

        public LarsaArray<LarsaPathElement> path { get { return new LarsaArray<LarsaPathElement>(LarsaApp.GetProp(o, "path")); } }
    }

    public class LarsaPathElement
    {
        public object o;
        public LarsaPathElement() { o = LarsaApp.CreateObject("LarsaElements", "clsPathElement"); }
        public LarsaPathElement(object obj) { o = obj; }
        public PathType pathType { get { return (PathType)LarsaApp.GetProp(o, "pathType"); } set { LarsaApp.SetProp(o, "pathType", value); } }
        public ElementType elementType { get { return (ElementType)LarsaApp.GetProp(o, "elementType"); } set { LarsaApp.SetProp(o, "elementType", value); } }
        public double offsetX { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "offset"), "x"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "offset"), "x", value); } }
        public double offsetY { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "offset"), "y"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "offset"), "y", value); } }
        public double offsetZ { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "offset"), "z"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "offset"), "z", value); } }
        public long elementNumber
        {
            get
            {
                var n1 = (int)LarsaApp.GetProp(o, "elementNumber", 1);
                var n2 = (int)LarsaApp.GetProp(o, "elementNumber", 2);
                if (n1 == n2 || n2 == -1)
                    return n1;
                else
                    throw new InvalidOperationException("The Path Element holds a range of elements.");
            }
            set
            {
                // clsPathElements hold a range of elements, but the range
                // can be a single element by setting the second index to -1.
                LarsaApp.SetProp(o, "elementNumber", 1, value);
                LarsaApp.SetProp(o, "elementNumber", 2, -1);
            }
        }
        public Tuple<int, int> elementNumberRange
        {
            get
            {
                var n1 = (int)LarsaApp.GetProp(o, "elementNumber", 1);
                var n2 = (int)LarsaApp.GetProp(o, "elementNumber", 2);
                return Tuple.Create(n1, n2);
            }
            set
            {
                LarsaApp.SetProp(o, "elementNumber", 1, value.Item1);
                LarsaApp.SetProp(o, "elementNumber", 2, value.Item2);
            }
        }
        public enum ElementType
        {
            Member = 0,
            Plate = 1, // only partially supported for tendons
            Solid = 2 // not supported yet but hopefully one day
        }
        public enum PathType
        {
            Geometry = 0,
            PathOnly = 1,
            SpanMark = 2
        }
    }

    public class LarsaLoadCase
    {
        public object o;
        public LarsaLoadCase() { o = LarsaApp.CreateObject("LarsaElements", "clsLoadCase"); }
        public LarsaLoadCase(object obj) { o = obj; }
        public int Number { get { return (int)LarsaApp.GetProp(o, "number"); } set { LarsaApp.SetProp(o, "number", value); } }
        public string Name { get { return (string)LarsaApp.GetProp(o, "name"); } set { LarsaApp.SetProp(o, "name", value); } }
        public LarsaArray<LarsaJointLoad> JointLoads { get { return new LarsaArray<LarsaJointLoad>(LarsaApp.GetProp(o, "jointLoads")); } }
        public LarsaArray<LarsaMemberLoad> MemberLoads { get { return new LarsaArray<LarsaMemberLoad>(LarsaApp.GetProp(o, "memberLoads")); } }
        public LarsaArray<LarsaPlateLoad> PlateLoads { get { return new LarsaArray<LarsaPlateLoad>(LarsaApp.GetProp(o, "plateLoads")); } }
        public double WeigthFactorX { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "weightFactor"), "x"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "weightFactor"), "x", value); } }
        public double WeigthFactorY { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "weightFactor"), "y"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "weightFactor"), "y", value); } }
        public double WeigthFactorZ { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "weightFactor"), "z"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "weightFactor"), "z", value); } }
    }

    public class LarsaJointLoad
    {
        public object o;
        public LarsaJointLoad() { o = LarsaApp.CreateObject("LarsaElements", "clsJointLoad"); }
        public LarsaJointLoad(object obj) { o = obj; }
        public int Number { get { return (int)LarsaApp.GetProp(o, "number"); } set { LarsaApp.SetProp(o, "number", value); } }
        public LarsaJoint Joint { get { return LarsaApp.GetObj<LarsaJoint>(LarsaApp.GetProp(o, "joint")); } set { LarsaApp.SetProp(o, "joint", LarsaApp.GetNativeObj(value)); } }
        public double Fx { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "Force"), "x"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "Force"), "x", value); } }
        public double Fy { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "Force"), "y"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "Force"), "y", value); } }
        public double Fz { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "Force"), "z"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "Force"), "z", value); } }
        public double Mx { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "Moment"), "x"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "Moment"), "x", value); } }
        public double My { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "Moment"), "y"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "Moment"), "y", value); } }
        public double Mz { get { return (double)LarsaApp.GetProp(LarsaApp.GetProp(o, "Moment"), "z"); } set { LarsaApp.SetProp(LarsaApp.GetProp(o, "Moment"), "z", value); } }
    }

    public enum LarsaMemberLoadTypes { PointForce = 0, UniformForce = 1, TrapezoidalForce = 2, PointMoment = 4, UniformMoment = 5, TrapezoidalMoment = 6, PreTension = 7, PostTension = 8, Elongation = 13 }
    public enum LarsaLoadDirections { LocalX = 0, LocalY = 1, LocalZ = 2, GlobalX = 3, GlobalY = 4, GlobalZ = 5, ProjectedX = 6, ProjectedY = 7, ProjectedZ = 8 }
    public class LarsaMemberLoad
    {
        public object o;
        public LarsaMemberLoad() { o = LarsaApp.CreateObject("LarsaElements", "clsMemberLoad"); }
        public LarsaMemberLoad(object obj) { o = obj; }
        public int Number { get { return (int)LarsaApp.GetProp(o, "number"); } set { LarsaApp.SetProp(o, "number", value); } }
        public LarsaMember Member { get { return LarsaApp.GetObj<LarsaMember>(LarsaApp.GetProp(o, "member")); } set { LarsaApp.SetProp(o, "member", LarsaApp.GetNativeObj(value)); } }
        public LarsaMemberLoadTypes Type { get { return (LarsaMemberLoadTypes)LarsaApp.GetProp(o, "loadType"); } set { LarsaApp.SetProp(o, "loadType", value); } }
        public LarsaLoadDirections Direction { get { return (LarsaLoadDirections)LarsaApp.GetProp(o, "loadDir"); } set { LarsaApp.SetProp(o, "loadDir", value); } }
        public double StartForce { get { return (double)LarsaApp.GetProp(o, "startW"); } set { LarsaApp.SetProp(o, "startW", value); } }
        public double EndForce { get { return (double)LarsaApp.GetProp(o, "endW"); } set { LarsaApp.SetProp(o, "endW", value); } }
        public double StartDistance { get { return (double)LarsaApp.GetProp(o, "startDist"); } set { LarsaApp.SetProp(o, "startDist", value); } }
        public double EndDistance { get { return (double)LarsaApp.GetProp(o, "endDist"); } set { LarsaApp.SetProp(o, "endDist", value); } }
    }

    public enum LarsaPlateLoadTypes { Uniform = 0, Point = 1 }
    public class LarsaPlateLoad
    {
        public object o;
        public LarsaPlateLoad() { o = LarsaApp.CreateObject("LarsaElements", "clsPlateLoad"); }
        public LarsaPlateLoad(object obj) { o = obj; }
        public int Number { get { return (int)LarsaApp.GetProp(o, "number"); } set { LarsaApp.SetProp(o, "number", value); } }
        public LarsaPlate Plate { get { return LarsaApp.GetObj<LarsaPlate>(LarsaApp.GetProp(o, "plate")); } set { LarsaApp.SetProp(o, "plate", LarsaApp.GetNativeObj(value)); } }

        public LarsaPlateLoadTypes Type { get { return (LarsaPlateLoadTypes)LarsaApp.GetProp(o, "loadType"); } set { LarsaApp.SetProp(o, "loadType", value); } }
        public LarsaLoadDirections Direction { get { return (LarsaLoadDirections)LarsaApp.GetProp(o, "loadDir"); } set { LarsaApp.SetProp(o, "loadDir", value); } }
        public double Load { get { return (double)LarsaApp.GetProp(o, "load"); } set { LarsaApp.SetProp(o, "load", value); } }
        public double PointDistX { get { return (double)LarsaApp.GetProp(o, "pointDistX"); } set { LarsaApp.SetProp(o, "pointDistX", value); } }
        public double PointDistY { get { return (double)LarsaApp.GetProp(o, "pointDistY"); } set { LarsaApp.SetProp(o, "pointDistY", value); } }
        public double TempChange { get { return (double)LarsaApp.GetProp(o, "tempChange"); } set { LarsaApp.SetProp(o, "tempChange", value); } }
        public double TempGradient { get { return (double)LarsaApp.GetProp(o, "tempGradient"); } set { LarsaApp.SetProp(o, "tempGradient", value); } }
    }

    public class LarsaLoadCombination
    {
        public object o;
        public LarsaLoadCombination() { o = LarsaApp.CreateObject("LarsaElements", "clsLoadCombination"); }
        public LarsaLoadCombination(object obj) { o = obj; }
        public int Number { get { return (int)LarsaApp.GetProp(o, "number"); } set { LarsaApp.SetProp(o, "number", value); } }
        public string Name { get { return (string)LarsaApp.GetProp(o, "name"); } set { LarsaApp.SetProp(o, "name", value); } }
        public LarsaArray<LarsaLoadCombinationRecord> LoadCases { get { return new LarsaArray<LarsaLoadCombinationRecord>(LarsaApp.GetProp(o, "primaryCases")); } }
    }

    public class LarsaLoadCombinationRecord
    {
        public object o;
        public LarsaLoadCombinationRecord() { o = LarsaApp.CreateObject("LarsaElements", "clsLoadCombinationRecord"); }
        public LarsaLoadCombinationRecord(object obj) { o = obj; }
        public int Number { get { return (int)LarsaApp.GetProp(o, "number"); } set { LarsaApp.SetProp(o, "number", value); } }
        public LarsaLoadCase LoadCase { get { return LarsaApp.GetObj<LarsaLoadCase>(LarsaApp.GetProp(o, "primaryCase")); } set { LarsaApp.SetProp(o, "primaryCase", LarsaApp.GetNativeObj(value)); } }
        public double LoadFactor { get { return (double)LarsaApp.GetProp(o, "loadFactor"); } set { LarsaApp.SetProp(o, "loadFactor", value); } }
    }

    public enum GEOMETRY_GROUP_ARRAYS { GGA_ALL = 0, GGA_JOINTS = 1, GGA_MEMBERS = 2, GGA_PLATES = 3, GGA_SPRINGS = 4, GGA_ISOLATORS = 5, GGA_SOLIDS = 6, }
    public class LarsaGroup
    {
        public object o;
        public LarsaGroup() { o = LarsaApp.CreateObject("LarsaElements", "clsGeoGroup"); }
        public LarsaGroup(object obj) { o = obj; }
        public string Name { get { return (string)LarsaApp.GetProp(o, "name"); } set { LarsaApp.SetProp(o, "name", value); } }
        public int Number { get { return (int)LarsaApp.GetProp(o, "number"); } set { LarsaApp.SetProp(o, "number", value); } }

        public void Clear(GEOMETRY_GROUP_ARRAYS type) { LarsaApp.CallMethod(o, "Clear", type); }
        public void Add(GEOMETRY_GROUP_ARRAYS type, int id) { LarsaApp.CallMethod(o, "addObject", type, id); }
        public void Remove(GEOMETRY_GROUP_ARRAYS type, int id) { int index = (int)LarsaApp.CallMethod(o, "findIndex", type, id); LarsaApp.CallMethod(o, "removeObject", type, index); }
    }
}
