// Branch-aware layout for OutSystems 11 server actions.
// Paste as the applyModelApiCode `code` argument. Replace the Layout(...) names.
// imports: OutSystems.Model, OutSystems.Model.Logic, OutSystems.Model.Logic.Nodes
// The host appends eSpace.Save. Do not call Save. The text must end at the final brace.
// Positions only. Do not change connectors, conditions, or expressions.
// Verified on Danfe_Create (40 nodes) and IsDanfeAlreadyRegistered (5 nodes):
// 0 shared cells, 0 upward forward edges, 0 TrueChange errors.
// Replace SuaServerAction. Add one Layout(...) call per action to arrange.

eSpace => {
    void Layout(OutSystems.Model.Logic.IServerAction act) {
        List<(OutSystems.Model.Logic.Nodes.IActionNode t, int dc, int dr)> Succ(OutSystems.Model.Logic.Nodes.IActionNode n) {
            var r = new List<(OutSystems.Model.Logic.Nodes.IActionNode, int, int)>();
            if (n is OutSystems.Model.Logic.Nodes.IIfNode i) {
                if (i.TrueTarget != null) r.Add((i.TrueTarget, 0, 1));
                if (i.FalseTarget != null) r.Add((i.FalseTarget, 1, 1));
            } else if (n is OutSystems.Model.Logic.Nodes.ISwitchNode s) {
                int k = 1;
                foreach (var c in s.Conditions) if (c.Target != null) r.Add((c.Target, k++, 1));
                if (s.OtherwiseTarget != null) r.Add((s.OtherwiseTarget, 0, 1));
            } else if (n is OutSystems.Model.Logic.Nodes.IForEachNode f) {
                if (f.CycleTarget != null) r.Add((f.CycleTarget, 1, 0));
                if (f.Target != null) r.Add((f.Target, 0, 1));
            } else {
                foreach (var c in n.Connectors) if (c.Target != null) r.Add((c.Target, 0, 1));
            }
            return r;
        }
        var col = new Dictionary<OutSystems.Model.Logic.Nodes.IActionNode, int>();
        var row = new Dictionary<OutSystems.Model.Logic.Nodes.IActionNode, int>();
        var used = new HashSet<(int, int)>();
        var back = new HashSet<(OutSystems.Model.Logic.Nodes.IActionNode, OutSystems.Model.Logic.Nodes.IActionNode)>();
        void Place(OutSystems.Model.Logic.Nodes.IActionNode root, int rootCol) {
            var state = new Dictionary<OutSystems.Model.Logic.Nodes.IActionNode, int>();
            void Dfs(OutSystems.Model.Logic.Nodes.IActionNode n) {
                state[n] = 1;
                foreach (var e in Succ(n)) {
                    if (col.ContainsKey(e.t)) continue;
                    if (!state.ContainsKey(e.t)) Dfs(e.t);
                    else if (state[e.t] == 1) back.Add((n, e.t));
                }
                state[n] = 2;
            }
            Dfs(root);
            var preds = state.Keys.ToDictionary(n => n, n => new List<(OutSystems.Model.Logic.Nodes.IActionNode p, int dc, int dr)>());
            foreach (var n in state.Keys)
                foreach (var e in Succ(n))
                    if (state.ContainsKey(e.t) && !back.Contains((n, e.t))) preds[e.t].Add((n, e.dc, e.dr));
            var pending = new List<OutSystems.Model.Logic.Nodes.IActionNode>(state.Keys);
            while (pending.Count > 0) {
                OutSystems.Model.Logic.Nodes.IActionNode n = null;
                foreach (var x in pending) {
                    var ready = true;
                    foreach (var p in preds[x]) if (!col.ContainsKey(p.p)) ready = false;
                    if (ready) { n = x; break; }
                }
                if (n == null) break;
                pending.Remove(n);
                int c = rootCol;
                int rw = 0;
                var hasPred = false;
                foreach (var p in preds[n]) {
                    int pc = col[p.p] + p.dc;
                    int pr = row[p.p] + p.dr;
                    if (!hasPred || pc < c) c = pc;
                    if (!hasPred || pr > rw) rw = pr;
                    hasPred = true;
                }
                while (used.Contains((c, rw))) c++;
                col[n] = c; row[n] = rw; used.Add((c, rw));
            }
        }
        OutSystems.Model.Logic.Nodes.IStartNode start = null;
        foreach (var n in act.Nodes) {
            var st = n as OutSystems.Model.Logic.Nodes.IStartNode;
            if (st != null) start = st;
        }
        if (start == null) throw new Exception("sem start " + act.Name);
        Place(start, 0);
        foreach (var n in act.Nodes) {
            var handler = n as OutSystems.Model.Logic.Nodes.IExceptionHandlerNode;
            if (handler != null && !col.ContainsKey(handler)) {
                int max = 0;
                foreach (var v in col.Values) if (v > max) max = v;
                Place(handler, max + 2);
            }
        }
        var upward = new List<string>();
        foreach (var n in col.Keys)
            foreach (var e in Succ(n))
                if (col.ContainsKey(e.t) && !back.Contains((n, e.t)) && row[e.t] < row[n])
                    upward.Add(n.GetType().Name + "->" + e.t.GetType().Name);
        if (used.Count != col.Count || upward.Count > 0) throw new Exception("LAYOUT-CHECK failed: " + string.Join(",", upward));
        foreach (var n in col.Keys) {
            n.HorizontalPosition = 3200 + col[n] * 1828;
            n.VerticalPosition = 800 + row[n] * 1371;
        }
        int margin = 0;
        foreach (var v in col.Values) if (v > margin) margin = v;
        margin = margin + 2;
        int cr = 0;
        foreach (var n in act.Nodes) {
            if (col.ContainsKey(n)) continue;
            while (used.Contains((margin, cr))) cr++;
            n.HorizontalPosition = 3200 + margin * 1828;
            n.VerticalPosition = 800 + cr * 1371;
            used.Add((margin, cr));
            cr++;
        }
    }
    Layout(eSpace.ServerActions.Named("SuaServerAction"));
}
