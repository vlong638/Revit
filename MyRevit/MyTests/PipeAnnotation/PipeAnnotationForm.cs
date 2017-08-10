using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using PmSoft.Common.CommonClass;
using PmSoft.Common.Controls;
using PmSoft.Optimization.DrawingProduction.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace PmSoft.Optimization.DrawingProduction
{
    public enum ActionType
    {
        Idle,
        SelectSinglePipe,
        SelectMultiplePipe,
    }

    public partial class PipeAnnotationForm : System.Windows.Forms.Form
    {
        private PipeAnnotationCmd PipeAnnotationCmd;
        private Document Doc { get { return PipeAnnotationCmd.Document; } }
        private Autodesk.Revit.DB.View View { get { return PipeAnnotationCmd.Document.ActiveView; } }
        public ActionType ActionType { set; get; }

        public PipeAnnotationForm(PipeAnnotationCmd pipeAnnotationCmd)
        {
            InitializeComponent();

            this.Shown += PipeAnnotationForm_Shown;
            this.FormClosing += PipeAnnotationForm_FormClosing;
            rb_OnPipe.CheckedChanged += Rb_OnPipe_CheckedChanged;
            KeyPress += PipeAnnotationForm_KeyPress;
            //暂为实现的功能
            cb_IsAutoPreventCollision.Enabled = false;
            cb_IncludeLinkPipe.Enabled = false;
            cb_BackGroupForSingle.Enabled = false;
            cb_BackGroupForMultiple.Enabled = false;

            LoadCmdData(pipeAnnotationCmd);
        }

        KeyBoardHook KeyBoarHook;
        private void PipeAnnotationForm_Shown(object sender, EventArgs e)
        {
            if (ActionType != ActionType.Idle && SelectedElementIds.Count() > 0)
            {
                SelectedElementIds.Clear();
                DialogResult = DialogResult.Retry;
                return;
            }

            //加载钩子
            KeyBoarHook = new KeyBoardHook();
            KeyBoarHook.SetHook();
            KeyBoarHook.OnKeyDownEvent += KeyBoarHook_OnKeyDownEvent;
        }

        private int KeyBoarHook_OnKeyDownEvent(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                return 1;
            }
            return 0;
        }

        private void PipeAnnotationForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (int)Keys.Escape)
                this.DialogResult = DialogResult.Cancel;
        }

        private void LoadCmdData(PipeAnnotationCmd pipeAnnotationCmd)
        {
            PipeAnnotationCmd = pipeAnnotationCmd;
            switch (PipeAnnotationCmd.PipeAnnotationUIData.SettingForSingle.Location)
            {
                case SinglePipeTagLocation.OnPipe:
                    rb_OnPipe.Checked = true;
                    break;
                case SinglePipeTagLocation.AbovePipe:
                    rb_AbovePipe.Checked = true;
                    break;
                default:
                    throw new NotImplementedException("暂未实现该类型:" + PipeAnnotationCmd.PipeAnnotationUIData.SettingForSingle.Location.ToString());
            }
            cb_Lead.Checked = PipeAnnotationCmd.PipeAnnotationUIData.SettingForSingle.NeedLeader;
            tb_LengthFromLine.Text = PipeAnnotationCmd.PipeAnnotationUIData.SettingForSingle.LengthFromLine_Milimeter.ToString();
            switch (PipeAnnotationCmd.PipeAnnotationUIData.SettingForMultiple.Location)
            {
                case MultiPipeTagLocation.OnLineEdge:
                    rb_OnLineEdge.Checked = true;
                    break;
                case MultiPipeTagLocation.OnLine:
                    rb_OnLine.Checked = true;
                    break;
                default:
                    throw new NotImplementedException("暂未实现该类型:" + PipeAnnotationCmd.PipeAnnotationUIData.SettingForMultiple.Location.ToString());
            }
            tb_LengthBetweenPipe.Text = PipeAnnotationCmd.PipeAnnotationUIData.SettingForMultiple.LengthBetweenPipe_Milimeter.ToString();
            cb_IncludeLinkPipe.Checked = PipeAnnotationCmd.PipeAnnotationUIData.SettingForCommon.IncludeLinkPipe;
            cb_IsAutoPreventCollision.Checked = PipeAnnotationCmd.PipeAnnotationUIData.SettingForCommon.AutoPreventCollision;
            //0728长度过滤
            cb_FilterVertical.Checked = PipeAnnotationCmd.PipeAnnotationUIData.SettingForCommon.FilterVertical;
            tb_MinLength.Text = PipeAnnotationCmd.PipeAnnotationUIData.SettingForCommon.MinLength_Milimeter.ToString();
        }

        /// <summary>
        /// 文字位于管道,勾选
        /// </summary> 
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Rb_OnPipe_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_OnPipe.Checked)
            {
                tb_LengthFromLine.Text = "0";
                tb_LengthFromLine.Enabled = false;
                cb_Lead.Checked = false;
                cb_Lead.Enabled = false;
            }
            else
            {
                tb_LengthFromLine.Enabled = true;
                cb_Lead.Enabled = true;
            }
        }

        public List<ElementId> SelectedElementIds = new List<ElementId>();
        /// <summary>
        /// 结束Revit控件选择
        /// </summary>
        /// <param name="selectedElementId"></param>
        public void FinishSelection()
        {
            var doc = PipeAnnotationCmd.UIApplication.ActiveUIDocument.Document;
            switch (ActionType)
            {
                case ActionType.SelectSinglePipe:
                    DelegateHelper.DelegateTransaction(doc, "生成单管直径标注", () =>
                    {
                        var pipe = doc.GetElement(SelectedElementIds.First()) as Pipe;
                        var setting = PipeAnnotationCmd.PipeAnnotationUIData.SettingForSingle;
                        var headerPoint = setting.GetHeaderPoint(pipe);
                        var tag = doc.Create.NewTag(View, pipe, setting.NeedLeader, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, headerPoint);
                        tag.TagHeadPosition = headerPoint;
                        return true;
                    });
                    break;
                case ActionType.SelectMultiplePipe:
                    if (SelectedElementIds.Count < 2)
                    {
                        TaskDialog.Show("警告", "多管直径标注需选择两个以上的管道");
                    }
                    else
                    {
                        DelegateHelper.DelegateTransaction(doc, "生成多管直径标注", () =>
                        {
                            new AnnotationCreater().GenerateMultipleTagSymbol(Doc, SelectedElementIds, PipeAnnotationCmd.PipeAnnotationUIData.SettingForMultiple, true);
                            return true;
                        });
                    }
                    break;
                default:
                    throw new NotImplementedException("暂未实现该类型的动作:" + ActionType.ToString());
            }
            if (SelectedElementIds.Count > 0)
                DialogResult = DialogResult.Retry;
            else
                ActionType = ActionType.Idle;
        }

        /// <summary>
        /// 关闭,存储处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PipeAnnotationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!CheckInputsOfSingle())
                return;
            if (!CheckInputsOfMultiple())
                return;
            if (!CheckInputsOfCommon())
                return;
            

            //int lengthFromLine;
            //if (int.TryParse(tb_LengthFromLine.Text, out lengthFromLine))
            //{
            //    PipeAnnotationCmd.PipeAnnotationUIData.SettingForSingle.LengthFromLine_Milimeter = lengthFromLine;
            //}
            //PipeAnnotationCmd.PipeAnnotationUIData.SettingForSingle.Location = rb_OnPipe.Checked ? SinglePipeTagLocation.OnPipe : SinglePipeTagLocation.AbovePipe;
            //PipeAnnotationCmd.PipeAnnotationUIData.SettingForSingle.NeedLeader = cb_Lead.Checked;
            ////Multiple
            //int lengthBetweenPipe;
            //if (int.TryParse(tb_LengthBetweenPipe.Text, out lengthBetweenPipe))
            //{
            //    PipeAnnotationCmd.PipeAnnotationUIData.SettingForMultiple.LengthBetweenPipe_Milimeter = lengthBetweenPipe;
            //}
            //PipeAnnotationCmd.PipeAnnotationUIData.SettingForMultiple.Location = rb_OnLineEdge.Checked ? MultiPipeTagLocation.OnLineEdge : MultiPipeTagLocation.OnLine;
            //Common
            PipeAnnotationCmd.PipeAnnotationUIData.SettingForCommon.IncludeLinkPipe = cb_IncludeLinkPipe.Checked;
            PipeAnnotationCmd.PipeAnnotationUIData.SettingForCommon.AutoPreventCollision = cb_IsAutoPreventCollision.Checked;
            //0728长度过滤



            if (KeyBoarHook != null)
                KeyBoarHook.UnHook();
        }

        /// <summary>
        /// 单管选管标注
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_PickSinglePipe_Click(object sender, EventArgs e)
        {
            if (!CheckInputsOfSingle())
                return;
            if (!CheckInputsOfCommon())
                return;

            ActionType = ActionType.SelectSinglePipe;
            DialogResult = DialogResult.Retry;
        }

        /// <summary>
        /// 单管一键标注
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>X
        private void btn_AllSinglePipe_Click(object sender, EventArgs e)
        {
            if (!CheckInputsOfSingle())
                return;
            if (!CheckInputsOfCommon())
                return;

            var doc = Doc;
            GenerateSinglePipeTag(doc, doc);
            ////链接进来的管道
            //var linkedDocs = new FilteredElementCollector(Doc).OfClass(typeof(RevitLinkInstance)).Select(p => p as RevitLinkInstance);
            //if (PipeAnnotationCmd.PipeAnnotationUIData.SettingForCommon.IncludeLinkPipe)
            //{
            //    foreach (RevitLinkInstance link in linkedDocs)
            //    {
            //        var linkDoc = link.GetLinkDocument();
            //        if (linkDoc == null)
            //            continue;

            //        GenerateSinglePipeTag(Doc, linkDoc);
            //    }
            //}
        }

        /// <summary>
        /// 生成单管选管标注
        /// </summary>
        /// <param name="datadoc"></param>
        private void GenerateSinglePipeTag(Document savedoc, Document datadoc)
        {
            DelegateHelper.DelegateTransaction(savedoc, "一键单管直径标注", () =>
            {
                var pipeIdInThisViews = new FilteredElementCollector(datadoc, View.Id).OfCategory(BuiltInCategory.OST_PipeTags)
                .WhereElementIsNotElementType()
                .Select(c => (c as IndependentTag).TaggedLocalElementId).ToList().Distinct();
                var pipes = new FilteredElementCollector(datadoc).OfCategory(BuiltInCategory.OST_PipeCurves).WhereElementIsNotElementType().ToList();
                foreach (var pipe in pipes)
                {
                    //0728长度过滤
                    if (!CheckFilter(pipe))
                        continue;
                    if (!pipeIdInThisViews.Contains(pipe.Id))
                    {
                        var setting = PipeAnnotationCmd.PipeAnnotationUIData.SettingForSingle;
                        var headerPoint = setting.GetHeaderPoint(pipe as Pipe);
                        var tag = savedoc.Create.NewTag(View, pipe, setting.NeedLeader, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, headerPoint);
                    }
                }
                return true;
            });
        }

        /// <summary>
        /// 检测通过返回true,未通过返回false
        /// </summary>
        /// <param name="pipe"></param>
        /// <returns></returns>
        private bool CheckFilter(Element pipe)
        {
            var curve = pipe.Location as LocationCurve;
            if (PipeAnnotationCmd.PipeAnnotationUIData.SettingForCommon.MinLength_Milimeter > 0
            && curve.Curve.Length < PipeAnnotationCmd.PipeAnnotationUIData.SettingForCommon.MinLength_Inch)
                return false;
            if (PipeAnnotationCmd.PipeAnnotationUIData.SettingForCommon.FilterVertical
            && Math.Abs(View.ViewDirection.DotProduct((curve.Curve as Line).Direction)) >= 0.999)//几乎垂直当前面
                return false;
            return true;
        }

        /// <summary>
        /// 多管选管标注
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_PickMultiPipe_Click(object sender, EventArgs e)
        {
            if (!CheckInputsOfMultiple())
                return;

            ActionType = ActionType.SelectMultiplePipe;
            DialogResult = DialogResult.Retry;
        }

        /// <summary>
        /// SlopePipes的集合
        /// </summary>
        class PipeCollection : List<SlopePipes>
        {
            public static XYZ ReferenceDirection = new XYZ(1, 0, 0);

            public void Add(Pipe pipe)
            {
                var parallelVector = ((pipe.Location as LocationCurve).Curve as Line).Direction;
                parallelVector = new XYZ(parallelVector.X, parallelVector.Y, 0);
                parallelVector = LocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour);
                var slope = Math.Round(parallelVector.AngleTo(ReferenceDirection), 2);
                var slopePipes = this.FirstOrDefault(c => c.Slope == slope);
                if (slopePipes == null)
                    Add(new SlopePipes(slope, pipe));
                else
                {
                    var pipePoint = LocationHelper.ToSameZ((pipe.Location as LocationCurve).Curve.GetEndPoint(0), slopePipes.ReferencePoint0);
                    var projectPoint = slopePipes.ReferenceLine.Project(pipePoint).XYZPoint;
                    var line = pipePoint - projectPoint;
                    var skew = new XYZ(line.X, line.Y, 0).GetLength();
                    slopePipes.SkewPipes.Add(new SkewPipe(skew, pipe));
                }
            }

            public List<PipeGroup> GetPipeGroups(double lengthBetweenPipe_Inch)
            {
                PipeGroupCollection pipeGroups = new PipeGroupCollection();
                foreach (var slopePipes in this)
                    pipeGroups.AddRange(slopePipes.GetPipeGroups(lengthBetweenPipe_Inch));
                return pipeGroups;
            }

            public void SortBySkew()
            {
                foreach (var scopePipes in this)
                {
                    scopePipes.SkewPipes = scopePipes.SkewPipes.OrderByDescending(c => c.Skew).ToList();
                }
            }
        }

        /// <summary>
        /// 某斜率对应的Pipe集合
        /// </summary>
        class SlopePipes
        {
            /// <summary>
            /// 斜率
            /// </summary>
            public double Slope { set; get; }
            public Line ReferenceLine { set; get; }
            public XYZ ReferencePoint0 { set; get; }
            public XYZ ReferencePoint1 { set; get; }
            public XYZ ReferenceDirection { set; get; }
            public List<SkewPipe> SkewPipes { set; get; }

            public SlopePipes(double slope, Pipe pipe)
            {
                Slope = slope;
                ReferenceLine = (pipe.Location as LocationCurve).Curve as Line;
                ReferencePoint0 = ReferenceLine.GetEndPoint(0);
                ReferencePoint1 = ReferenceLine.GetEndPoint(1);
                ReferenceDirection = ReferenceLine.Direction;
                ReferenceLine.MakeUnbound();
                SkewPipes = new List<SkewPipe>();
                SkewPipes.Add(new SkewPipe(0, pipe));
            }

            /// <summary>
            /// 重叠检测器
            /// </summary>
            class OverlapChecker
            {
                List<OverlapBranch> Branches { set; get; }
                SkewPipe LatestSkewPipe { set; get; }
                double LengthBetweenPipe { set; get; }

                public OverlapChecker(SkewPipe skewPipe, double lengthBetweenPipe)
                {
                    LengthBetweenPipe = lengthBetweenPipe;
                    Branches = new List<OverlapBranch>();
                    Push(skewPipe);

                    //Branches.Add(new OverlapBranch(skewPipe));
                    //LatestSkewPipe = skewPipe;
                }

                /// <summary>
                /// 清空
                /// </summary>
                public PipeGroupCollection Clear()
                {
                    PipeGroupCollection collection = new PipeGroupCollection();
                    foreach (var branch in Branches)
                        collection.Add(branch.ToPipeGroup());
                    Branches.Clear();
                    LatestSkewPipe = null;
                    return collection;
                }

                /// <summary>
                /// 加入队列
                /// </summary>
                /// <param name="skewPipe"></param>
                public PipeGroupCollection Push(SkewPipe skewPipe)
                {
                    var skewPipeId = skewPipe.pipe.Id.IntegerValue;
                    if (LatestSkewPipe == null)
                    {
                        LatestSkewPipe = skewPipe;
                        Branches.Add(new OverlapBranch(skewPipe));
                        return null;
                    }
                    //如果和最近一次添加项的间距超出限制,则所有的都超出限制,直接做全部结算
                    if (!IsValidInterval(LatestSkewPipe, skewPipe))
                    {
                        var result = Clear();
                        Push(skewPipe);
                        return result;
                    }

                    PipeGroupCollection collection = new PipeGroupCollection();
                    LatestSkewPipe = skewPipe;
                    //对于每一个分支,最近添加项都有可能是不同的,需重新计算间距
                    bool isExist = false;
                    if (Branches.Count > 100000)
                    {
                        throw new NotImplementedException("超出了十万的分支限制");
                    }
                    for (int i = Branches.Count() - 1; i >= 0; i--)
                    {
                        var branch = Branches[i];
                        if (branch.SkewPipes.Count == AnnotationConstaints.PipeCountMax - 1)//如果分组个数超出了规定 则不予生成
                        {
                            throw new NotImplementedException("管道超出了并排6个的上限");
                            branch.Push(skewPipe);
                            collection.Add(branch.ToPipeGroup());
                            Branches.Remove(branch);
                        }
                        else if (!IsValidInterval(branch.LatestSkewPipe, skewPipe))//如果间距超出了限制,则做一波结算
                        {
                            collection.Add(branch.ToPipeGroup());
                            Branches.Remove(branch);
                        }
                        else
                        {
                            branch.Push(skewPipe);
                            if (branch.IsOverlapped)
                                Branches.Add(branch.Clone());
                            else
                            {
                                //TODO 如果已有该项的单项
                                if (!isExist)
                                {
                                    foreach (var b in Branches)
                                    {
                                        if (b.SkewPipes.Count() == 1 && b.SkewPipes[0] == skewPipe)
                                        {
                                            isExist = true;
                                            break;
                                        }
                                    }
                                    if (!isExist)
                                        Branches.Add(new OverlapBranch(skewPipe));
                                }
                            }
                            branch.PopLast();
                        }
                    }
                    //本来想要在这里做消重处理 比如 A-B A-B-D但是后续可能出现A-B-E
                    //目前没有很好的办法检测后续是否会出现E
                    return collection;
                }

                /// <summary>
                /// 是否间距有效
                /// </summary>
                /// <param name="previousPipe"></param>
                /// <param name="currentPipe"></param>
                /// <returns></returns>
                private bool IsValidInterval(SkewPipe previousPipe, SkewPipe currentPipe)
                {
                    return Math.Abs(previousPipe.Skew - currentPipe.Skew) <= LengthBetweenPipe;
                }
            }
            /// <summary>
            /// 分支
            /// </summary>
            class OverlapBranch
            {
                /// <summary>
                /// 重叠判断,左点的最右未越过右点最左
                /// </summary>
                public bool IsOverlapped
                {
                    get
                    {
                        var xRightOfLefts = Math.Round(Lefts.First(c => c.X == Lefts.Max(p => p.X)).X, 2);
                        var xLeftOfRights = Math.Round(Rights.First(c => c.X == Rights.Min(p => p.X)).X, 2);
                        return xRightOfLefts <= xLeftOfRights;
                    }
                }
                public SkewPipe LatestSkewPipe
                {
                    get
                    {

                        return SkewPipes.Last();
                    }
                }

                List<XYZ> Lefts { set; get; }
                List<XYZ> Rights { set; get; }
                public List<SkewPipe> SkewPipes { set; get; }
                Curve ReferenceCurve { set; get; }

                OverlapBranch()
                {
                    InitProperties();
                }
                public OverlapBranch(SkewPipe skewPipe)
                {
                    InitProperties();
                    ReferenceCurve = (skewPipe.pipe.Location as LocationCurve).Curve;
                    ReferenceCurve.MakeUnbound();
                    Push(skewPipe);
                }

                private void InitProperties()
                {
                    Lefts = new List<XYZ>();
                    Rights = new List<XYZ>();
                    SkewPipes = new List<SkewPipe>();
                }

                /// <summary>
                /// 加入队列
                /// </summary>
                /// <param name="skewPipe"></param>
                public void Push(SkewPipe skewPipe)
                {
                    //这里以X小的为左点,X大的为右点
                    SkewPipes.Add(skewPipe);
                    var curve = (skewPipe.pipe.Location as LocationCurve).Curve;
                    if (curve.GetEndPoint(0).X < curve.GetEndPoint(1).X)
                    {
                        Lefts.Add(ReferenceCurve.Project(curve.GetEndPoint(0)).XYZPoint);
                        Rights.Add(ReferenceCurve.Project(curve.GetEndPoint(1)).XYZPoint);
                    }
                    else
                    {
                        Lefts.Add(ReferenceCurve.Project(curve.GetEndPoint(1)).XYZPoint);
                        Rights.Add(ReferenceCurve.Project(curve.GetEndPoint(0)).XYZPoint);
                    }
                }

                /// <summary>
                /// 移除队首的
                /// </summary>
                public void PopFirst()
                {
                    if (SkewPipes.Count == 0)
                        return;

                    SkewPipes.RemoveAt(0);
                    Lefts.RemoveAt(0);
                    Rights.RemoveAt(0);
                }

                /// <summary>
                /// 移除队尾的
                /// </summary>
                public void PopLast()
                {
                    int index = SkewPipes.Count - 1;
                    if (index < 0)
                        return;

                    SkewPipes.RemoveAt(index);
                    Lefts.RemoveAt(index);
                    Rights.RemoveAt(index);
                }

                /// <summary>
                /// 清空
                /// </summary>
                public void Clear()
                {
                    SkewPipes.Clear();
                    Lefts.Clear();
                    Rights.Clear();
                }

                /// <summary>
                /// 克隆
                /// </summary>
                /// <returns></returns>
                public OverlapBranch Clone()
                {
                    var clone = new OverlapBranch();
                    clone.Lefts.AddRange(Lefts);
                    clone.Rights.AddRange(Rights);
                    clone.SkewPipes.AddRange(SkewPipes);
                    clone.ReferenceCurve = ReferenceCurve;
                    return clone;
                }

                public PipeGroup ToPipeGroup()
                {
                    return new PipeGroup(SkewPipes.Select(c => c.pipe));
                }
            }

            public List<PipeGroup> GetPipeGroups(double lengthBetweenPipe_Inch)
            {
                PipeGroupCollection pipeGroups = new PipeGroupCollection();
                SkewPipe start;
                SkewPipe pre, cur;
                start = pre = cur = SkewPipes[0];
                OverlapChecker overlap = new OverlapChecker(cur, lengthBetweenPipe_Inch);
                for (int i = 1; i < SkewPipes.Count(); i++)
                {
                    var result = overlap.Push(SkewPipes[i]);
                    if (result != null && result.Count != 0)
                        pipeGroups.AddRange(result);
                }
                pipeGroups.AddRange(overlap.Clear());
                return pipeGroups;
            }
        }

        /// <summary>
        /// 带偏移量的Pipe
        /// </summary>
        class SkewPipe
        {
            public SkewPipe(double skew, Pipe pipe)
            {
                Skew = skew;
                this.pipe = pipe;
            }

            /// <summary>
            /// 偏移
            /// </summary>
            public double Skew { set; get; }
            /// <summary>
            /// 管
            /// </summary>
            public Pipe pipe { set; get; }
        }

        class PipeGroupCollection : List<PipeGroup>
        {
            //
            public new void Add(PipeGroup pipeGroup)
            {
                for (int i = this.Count - 1; i >= 0; i--)
                {
                    //由于整体的计算处理是自上而下增量的
                    //即有的短于新增的,则检测既有的是否被新增的包含
                    //即有的长于新增的,则检测既有的是否包含新增的
                    var existGroup = this[i];
                    if (existGroup.Count() < pipeGroup.Count)
                    {
                        bool isContained = pipeGroup.Contains(existGroup);// CheckContains(pipeGroup, existGroup);
                        if (isContained)
                            base.Remove(existGroup);
                    }
                    else if (existGroup.Count() > pipeGroup.Count)
                    {
                        bool isContained = existGroup.Contains(pipeGroup);//CheckContains(existGroup, pipeGroup);
                        if (isContained)
                            return;
                    }
                }
                base.Add(pipeGroup);
            }

            private static bool CheckContains(PipeGroup longOne, PipeGroup shortOne)
            {
                int j = 0;
                for (int i = 0; i < shortOne.Count(); i++)
                {
                    var cur = shortOne[i];
                    for (; j < longOne.Count(); j++)
                    {
                        if (longOne[j] == cur)
                            break;
                    }
                    if (j == longOne.Count())
                    {
                        return false;
                    }
                }
                return true;
            }

            public new void AddRange(IEnumerable<PipeGroup> pipeGroups)
            {
                foreach (var pipeGroup in pipeGroups)
                {
                    Add(pipeGroup);
                }
            }
        }

        class PipeGroup : List<Pipe>
        {
            public PipeGroup(IEnumerable<Pipe> pipes)
            {
                this.AddRange(pipes);
            }

            public IEnumerable<ElementId> GetPipeIds()
            {
                return this.Select(c => c.Id);
            }

            public bool Contains(PipeGroup anotherGroup)
            {
                var longOne = this;
                var shortOne = anotherGroup;
                int j = 0;
                for (int i = 0; i < shortOne.Count(); i++)
                {
                    var cur = shortOne[i];
                    for (; j < longOne.Count(); j++)
                    {
                        if (longOne[j] == cur)
                            break;
                    }
                    if (j == longOne.Count())
                    {
                        return false;
                    }
                }
                return true;
            }
        }


        /// <summary>
        /// 多管一键标注
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_AllMulltiplePipe_Click(object sender, EventArgs e)
        {
            if (!CheckInputsOfMultiple())
                return;
            if (!CheckInputsOfSingle())
                return;
            if (!CheckInputsOfCommon())
                return;

            //先删除后新增
            var lines = new FilteredElementCollector(Doc, View.Id)
                .OfClass(typeof(FamilyInstance))
                .WherePasses(new FamilyInstanceFilter(Doc, PipeAnnotationContext.GetMultipleTagSymbol(Doc).Id));
            var pipeTags = new FilteredElementCollector(Doc, View.Id)
                .OfCategory(BuiltInCategory.OST_PipeTags)
                .WhereElementIsNotElementType();
            if (pipeTags.Count() > 0 || lines.Count() > 0)
                if (PMMessageBox.ShowQuestion("已存在标注,继续将重新生成所有管道标注", MessageBoxButtons.OKCancel) != DialogResult.OK)
                    return;
            DelegateHelper.DelegateTransaction(Doc, "清空已有标注", () =>
            {
                //清空模型对象
                var lineIds = lines.Select(c => c.Id).ToList();
                foreach (var lineId in lineIds)
                {
                    Doc.Delete(lineId);
                }
                var pipeTagIds = pipeTags.Select(c => c.Id).ToList();
                foreach (var pipeTagId in pipeTagIds)
                {
                    Doc.Delete(pipeTagId);
                }
                //删除存储的关联
                var storageCollection = PipeAnnotationContext.GetCollection(Doc);
                storageCollection.RemoveAll(c => c.ViewId == View.Id.IntegerValue);
                storageCollection.Save(Doc);
                var creator = new AnnotationCreater();
                //生成处理
                var pipes = new FilteredElementCollector(Doc, View.Id)
                    .OfCategory(BuiltInCategory.OST_PipeCurves)
                    .WhereElementIsNotElementType();
                PipeCollection collection = new PipeCollection();
                foreach (Pipe pipe in pipes)
                    if (CheckFilter(pipe))
                        collection.Add(pipe);
                collection.SortBySkew();
                List<PipeGroup> slopedPipeGroups = null;
                try
                {
                    slopedPipeGroups = collection.GetPipeGroups(PipeAnnotationCmd.PipeAnnotationUIData.SettingForMultiple.LengthBetweenPipe_Inch);
                }
                catch (Exception ex)//特定情况暂时以提示处理,比如连续超过7个,比如
                {
                    TaskDialog.Show("警告", ex.Message);
                    return false;
                }
                foreach (var pipeGroup in slopedPipeGroups)
                    if (pipeGroup.Count > 1)
                    {
                        creator.GenerateMultipleTagSymbol(Doc, pipeGroup.Select(c => c.Id), PipeAnnotationCmd.PipeAnnotationUIData.SettingForMultiple, false);
                    }
                    else
                    {
                        var pipe = Doc.GetElement(pipeGroup.First().Id) as Pipe;
                        var setting = PipeAnnotationCmd.PipeAnnotationUIData.SettingForSingle;
                        var headerPoint = setting.GetHeaderPoint(pipe);
                        var tag = Doc.Create.NewTag(View, pipe, setting.NeedLeader, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, headerPoint);
                        tag.TagHeadPosition = headerPoint;
                    }
                creator.FinishMultipleGenerate(Doc);
                return true;
            });
        }

        #region Checks
        /// <summary>
        /// 单管尺寸标注相关输入检测
        /// </summary>
        /// <returns></returns>
        private bool CheckInputsOfSingle()
        {
            int lengthFromLine;
            if (!int.TryParse(tb_LengthFromLine.Text, out lengthFromLine))
            {
                TaskDialog.Show("警告", "(标记距离管道边缘)需为有效的数字");
                return false;
            }
            PipeAnnotationCmd.PipeAnnotationUIData.SettingForSingle.LengthFromLine_Milimeter = lengthFromLine;
            PipeAnnotationCmd.PipeAnnotationUIData.SettingForSingle.Location = rb_OnPipe.Checked ? SinglePipeTagLocation.OnPipe : SinglePipeTagLocation.AbovePipe;
            PipeAnnotationCmd.PipeAnnotationUIData.SettingForSingle.NeedLeader = cb_Lead.Checked;

            PipeAnnotationCmd.PipeAnnotationUIData.SettingForCommon.IncludeLinkPipe = cb_IncludeLinkPipe.Checked;
            PipeAnnotationCmd.PipeAnnotationUIData.SettingForCommon.AutoPreventCollision = cb_IsAutoPreventCollision.Checked;
            return true;
        }

        /// <summary>
        /// 多管尺寸标注相关输入检测
        /// </summary>
        /// <returns></returns>
        private bool CheckInputsOfMultiple()
        {
            int lengthBetweenPipe;
            if (!int.TryParse(tb_LengthBetweenPipe.Text, out lengthBetweenPipe))
            {
                TaskDialog.Show("警告", "(平行多管尺寸间净距)需为有效的数字");
                return false;
            }
            PipeAnnotationCmd.PipeAnnotationUIData.SettingForMultiple.LengthBetweenPipe_Milimeter = lengthBetweenPipe;
            PipeAnnotationCmd.PipeAnnotationUIData.SettingForMultiple.Location = rb_OnLineEdge.Checked ? MultiPipeTagLocation.OnLineEdge : MultiPipeTagLocation.OnLine;

            PipeAnnotationCmd.PipeAnnotationUIData.SettingForCommon.IncludeLinkPipe = cb_IncludeLinkPipe.Checked;
            PipeAnnotationCmd.PipeAnnotationUIData.SettingForCommon.AutoPreventCollision = cb_IsAutoPreventCollision.Checked;
            return true;
        }


        /// <summary>
        /// 单管尺寸标注相关输入检测
        /// </summary>
        /// <returns></returns>
        private bool CheckInputsOfCommon()
        {
            int minLength;
            if (!int.TryParse(tb_MinLength.Text, out minLength))
            {
                TaskDialog.Show("警告", "(管道最短长度)需为有效的数字");
                return false;
            }
            PipeAnnotationCmd.PipeAnnotationUIData.SettingForCommon.MinLength_Milimeter = minLength;
            PipeAnnotationCmd.PipeAnnotationUIData.SettingForCommon.FilterVertical = cb_FilterVertical.Checked;
            PipeAnnotationCmd.PipeAnnotationUIData.SaveConfig(PipeAnnotationCmd.Document);
            return true;
        }
        #endregion
    }

    #region Settings
    /// <summary>
    /// 单管直径标注 位置类型
    /// </summary>
    public enum SinglePipeTagLocation
    {
        OnPipe,
        AbovePipe,
    }
    /// <summary>
    /// 单管直径标注 参数
    /// </summary>
    public class SinglePipeAnnotationSettings
    {
        public PipeAnnotationUIData Parent { set; get; }
        public bool NeedLeader { set; get; }
        public int LengthFromLine_Milimeter { set; get; }
        public double LengthFromLine_Inch { get { return UnitHelper.ConvertToInch(LengthFromLine_Milimeter, UnitType.millimeter); } }
        public SinglePipeTagLocation Location { set; get; }

        public SinglePipeAnnotationSettings(PipeAnnotationUIData parent)
        {
            Parent = parent;
        }

        /// <summary>
        /// 标注定位计算
        /// </summary>
        /// <param name="pipe"></param>
        /// <returns></returns>
        public XYZ GetHeaderPoint(Pipe pipe)
        {
            switch (Location)
            {
                case SinglePipeTagLocation.OnPipe:
                    var locationCurve = (pipe.Location as LocationCurve).Curve;
                    var midPoint = (locationCurve.GetEndPoint(0) + locationCurve.GetEndPoint(1)) / 2;
                    return midPoint; ;
                case SinglePipeTagLocation.AbovePipe:
                    locationCurve = (pipe.Location as LocationCurve).Curve;
                    midPoint = (locationCurve.GetEndPoint(0) + locationCurve.GetEndPoint(1)) / 2;
                    var parallelVector = (locationCurve as Line).Direction;
                    var verticalVector = LocationHelper.GetVerticalVectorForPlaneXYZ(parallelVector, LocationHelper.XYZAxle.Z);
                    verticalVector = LocationHelper.GetVectorByQuadrantForPlaneXYZ(verticalVector, QuadrantType.OneAndTwo,LocationHelper.XYZAxle.Z);
                    verticalVector = LocationHelper.ToUnitVector(verticalVector);
                    return midPoint + LengthFromLine_Inch * verticalVector;
                default:
                    throw new NotImplementedException("暂未支持该类型的定位计算:" + Location.ToString());
            }
        }
    }
    /// <summary>
    /// 多管直径标注 位置类型
    /// </summary>
    public enum MultiPipeTagLocation
    {
        OnLineEdge,
        OnLine,
    }
    /// <summary>
    /// 多管直径标注 参数
    /// </summary>
    public class MultiPipeAnnotationSettings
    {
        public PipeAnnotationUIData Parent { set; get; }
        public int LengthBetweenPipe_Milimeter { set; get; }
        public double LengthBetweenPipe_Inch { get { return UnitHelper.ConvertToInch(LengthBetweenPipe_Milimeter, UnitType.millimeter); } }
        public MultiPipeTagLocation Location { set; get; }

        public MultiPipeAnnotationSettings(PipeAnnotationUIData parent)
        {
            Parent = parent;
        }
    }
    /// <summary>
    /// 通用 参数
    /// </summary>
    public class CommonAnnotationSettings
    {
        public bool IncludeLinkPipe { set; get; }
        public bool AutoPreventCollision { set; get; }
        public bool FilterVertical { set; get; }
        public int MinLength_Milimeter { set; get; }
        public double MinLength_Inch { get { return UnitHelper.ConvertToInch(MinLength_Milimeter, UnitType.millimeter); } }
    }
    #endregion
}
