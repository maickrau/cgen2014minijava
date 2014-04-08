using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace cgen2014minijava
{
    public class NFA
    {
        private abstract class Connection
        {
            public override int GetHashCode()
            {
                return start ^ end;
            }
            public int start;
            public int end;
            public abstract Connection copy();
            public abstract bool accepts(char ch);
            public void increment(int amount)
            {
                start += amount;
                end += amount;
            }
            public abstract String debugString();
            public virtual HashSet<char> acceptSet()
            {
                return new HashSet<char>();
            }
            public virtual HashSet<char> interestingCharacters()
            {
                return acceptSet();
            }
        }
        private class AnythingConnection : Connection
        {
            public override bool Equals(Object second)
            {
                if (!(second is AnythingConnection))
                {
                    return false;
                }
                return start == ((AnythingConnection)second).start && end == ((AnythingConnection)second).end;
            }
            public AnythingConnection(int start, int end)
            {
                this.start = start;
                this.end = end;
            }
            public override Connection copy()
            {
                return new AnythingConnection(start, end);
            }
            public override bool accepts(char ch)
            {
                return true;
            }
            public override string debugString()
            {
                return new StringBuilder().Append(start).Append(" ").Append(end).Append(" anything").ToString();
            }
        }
        private class EpsilonConnection : Connection
        {
            public override bool Equals(Object second)
            {
                if (!(second is EpsilonConnection))
                {
                    return false;
                }
                return start == ((EpsilonConnection)second).start && end == ((EpsilonConnection)second).end;
            }
            public EpsilonConnection(int start, int end)
            {
                this.start = start;
                this.end = end;
            }
            public override Connection copy()
            {
                return new EpsilonConnection(start, end);
            }
            public override bool accepts(char ch)
            {
                return true;
            }
            public override string debugString()
            {
                return new StringBuilder().Append(start).Append(" ").Append(end).Append(" epsilon").ToString();
            }
        }
        private class ConnectionAnyOf : Connection
        {
            public override bool Equals(Object second)
            {
                if (!(second is ConnectionAnyOf))
                {
                    return false;
                }
                return start == ((ConnectionAnyOf)second).start && end == ((ConnectionAnyOf)second).end && allowed.SetEquals(((ConnectionAnyOf)second).allowed);
            }
            protected HashSet<char> allowed;
            public ConnectionAnyOf()
            {
                start = -1;
                end = -1;
                allowed = new HashSet<char>();
            }
            public ConnectionAnyOf(int start, int end, HashSet<char> allowed)
            {
                this.start = start;
                this.end = end;
                this.allowed = new HashSet<char>(allowed);
            }
            public ConnectionAnyOf(ConnectionAnyOf second)
            {
                start = second.start;
                end = second.end;
                allowed = new HashSet<char>(second.allowed);
            }
            public override Connection copy()
            {
                return new ConnectionAnyOf(this);
            }
            public override bool accepts(char ch)
            {
                return allowed.Contains(ch);
            }
            public override HashSet<char> acceptSet()
            {
                return new HashSet<char>(allowed);
            }
            public override string debugString()
            {
                StringBuilder str = new StringBuilder().Append(start).Append(" ").Append(end).Append(" ");
                foreach (char c in allowed)
                {
                    str.Append(c).Append(" ");
                }
                return str.ToString();
            }
        }
        private class ConnectionExactCharacter : ConnectionAnyOf
        {
            public ConnectionExactCharacter(int start, int end, char character)
            {
                this.start = start;
                this.end = end;
                this.allowed = new HashSet<char>();
                this.allowed.Add(character);
            }
        }
        private class ConnectionAnythingBut : Connection
        {
            public override bool Equals(Object second)
            {
                if (!(second is ConnectionAnythingBut))
                {
                    return false;
                }
                return start == ((ConnectionAnythingBut)second).start && end == ((ConnectionAnythingBut)second).end && forbidden.SetEquals(((ConnectionAnythingBut)second).forbidden);
            }
            public HashSet<char> forbidden;
            public ConnectionAnythingBut(int start, int end, HashSet<char> forbidden)
            {
                this.start = start;
                this.end = end;
                this.forbidden = new HashSet<char>(forbidden);
            }
            public ConnectionAnythingBut(ConnectionAnythingBut second)
            {
                this.start = second.start;
                this.end = second.end;
                this.forbidden = new HashSet<char>(second.forbidden);
            }
            public override bool accepts(char ch)
            {
                return !forbidden.Contains(ch);
            }
            public override Connection copy()
            {
                return new ConnectionAnythingBut(this);
            }
            public override string debugString()
            {
                StringBuilder str = new StringBuilder().Append(start).Append(" ").Append(end).Append(" not ");
                foreach (char c in forbidden)
                {
                    str.Append(c).Append(" ");
                }
                return str.ToString();
            }
            public override HashSet<char> interestingCharacters()
            {
                return new HashSet<char>(forbidden);
            }
        }

        private int startState;
        private int numStates;
        private List<Tuple<int, int>> currentStates;
        private HashSet<int> endStates;
        private List<Connection> connections;
        private List<List<Connection>> connectionPerState;
        private bool hasEpsilons;
        public void debugPrint()
        {
            System.Console.WriteLine(numStates);
            System.Console.WriteLine(startState);
            foreach (int i in endStates)
            {
                System.Console.Write("{0} ", i);
            }
            System.Console.WriteLine();
            foreach (Connection c in connections)
            {
                System.Console.WriteLine(c.debugString());
            }
        }
        public NFA()
        {
            endStates = new HashSet<int>();
            connections = new List<Connection>();
            connectionPerState = new List<List<Connection>>();
            connectionPerState.Add(new List<Connection>());
            startState = 0;
            numStates = 1;
            hasEpsilons = false;
        }
        public NFA(HashSet<char> range) : this()
        {
            //any one of these characters
            numStates++;
            foreach (char c in range)
            {
                connections.Add(new ConnectionExactCharacter(0, 1, c));
            }
            endStates.Add(1);
            calculateConnectionPerState();
        }
        public NFA(String str) : this()
        {
            //exactly this string
            for (int i = 0; i < str.Length; i++)
            {
                connections.Add(new ConnectionExactCharacter(i, i + 1, str[i]));
            }
            numStates = str.Length + 1;
            endStates.Add(str.Length);
            calculateConnectionPerState();
        }
        public NFA(int len) : this()
        {
            //any string with length len
            for (int i = 0; i < len; i++)
            {
                connections.Add(new AnythingConnection(i, i + 1));
            }
            numStates = len + 1;
            endStates.Add(len);
            calculateConnectionPerState();
        }
        public NFA(NFA second)
        {
            //we want to create a deep copy instead of modifying the original
            connections = second.connections.ConvertAll(c => c.copy());
            endStates = new HashSet<int>(second.endStates);
            startState = second.startState;
            numStates = second.numStates;
            hasEpsilons = second.hasEpsilons;
            calculateConnectionPerState();
        }
        private int addState(bool endState)
        {
            int ret = numStates;
            numStates++;
            connectionPerState.Add(new List<Connection>());
            if (endState)
            {
                endStates.Add(ret);
            }
            return ret;
        }
        private void connect(int start, int end, Connection connectionType)
        {
            Connection newConnection = connectionType.copy();
            newConnection.start = start;
            newConnection.end = end;
            connections.Add(newConnection);
            connectionPerState[start].Add(newConnection);
        }
        private void calculateConnectionPerState()
        {
            connectionPerState = new List<List<Connection>>();
            for (int i = 0; i < numStates; i++)
            {
                connectionPerState.Add(new List<Connection>());
            }
            foreach (Connection c in connections)
            {
                connectionPerState[c.start].Add(c);
            }
        }
        private List<char> connectionsFrom(int state)
        {
            List<char> ret = new List<char>();
            foreach (Connection c in connectionPerState[state])
            {
                ret.AddRange(c.acceptSet());
            }
            return ret;
        }
        private void removeEmptyAnythingButs()
        {
            for (int i = connections.Count-1; i >= 0; i--)
            {
                if (connections[i] is ConnectionAnythingBut)
                {
                    if (((ConnectionAnythingBut)connections[i]).forbidden.Count == 0)
                    {
                        connections.RemoveAt(i);
                    }
                }
            }
            calculateConnectionPerState();
        }
        private void removeDoubleConnections()
        {
            for (int a = 0; a < connections.Count; a++)
            {
                for (int b = connections.Count-1; b > a; b--)
                {
                    if (connections[a].Equals(connections[b]))
                    {
                        connections.RemoveAt(b);
                    }
                }
            }
            calculateConnectionPerState();
        }
        public NFA toDFA()
        {
            NFA temp = new NFA(this);
            NFA ret = new NFA();
            temp.deEpsilonate();
            List<HashSet<int>> newStates = new List<HashSet<int>>();
            newStates.Add(new HashSet<int>());
            Queue<HashSet<int>> statesThatNeedProcessing = new Queue<HashSet<int>>();
            HashSet<int> startState = new HashSet<int>();
            startState.Add(temp.startState);
            ret.startState = ret.addState(temp.endStates.Contains(temp.startState));
            newStates.Add(startState);
            statesThatNeedProcessing.Enqueue(startState);
            while (statesThatNeedProcessing.Count > 0)
            {
                HashSet<int> newState = statesThatNeedProcessing.Dequeue();
                int hasState = newStates.FindIndex(h => newState.SetEquals(h));
                List<Connection> allConnections = new List<Connection>();
                List<Connection> uninterestingConnections = new List<Connection>();
                HashSet<int> anyReachable = new HashSet<int>();
                foreach (int i in newState)
                {
                    allConnections.AddRange(temp.connectionPerState[i]);
                }
                HashSet<char> interestingChars = new HashSet<char>();
                foreach (Connection c in allConnections)
                {
                    if (c is AnythingConnection)
                    {
                        anyReachable.Add(c.end);
                    }
                    if (c is AnythingConnection || c is ConnectionAnythingBut)
                    {
                        uninterestingConnections.Add(c);
                    }
                    interestingChars.UnionWith(c.interestingCharacters());
                }
                foreach (char ch in interestingChars)
                {
                    HashSet<int> reachable = new HashSet<int>();
                    foreach (Connection c in allConnections)
                    {
                        if (c.accepts(ch))
                        {
                            reachable.Add(c.end);
                        }
                    }
                    int newAddIndex = newStates.FindIndex(h => reachable.SetEquals(h));
                    if (newAddIndex == -1)
                    {
                        bool endState = false;
                        foreach (int i in reachable)
                        {
                            if (temp.endStates.Contains(i))
                            {
                                endState = true;
                                break;
                            }
                        }
                        newAddIndex = ret.addState(endState);
                        newStates.Add(reachable);
                        statesThatNeedProcessing.Enqueue(reachable);
                    }
                    ret.connect(hasState, newAddIndex, new ConnectionExactCharacter(hasState, newAddIndex, ch));
                }
                if (uninterestingConnections.Count > 0)
                {
                    HashSet<int> reachable = new HashSet<int>();
                    foreach (Connection c in uninterestingConnections)
                    {
                        reachable.Add(c.end);
                    }
                    int newAddIndex = newStates.FindIndex(h => reachable.SetEquals(h));
                    if (newAddIndex == -1)
                    {
                        bool endState = false;
                        foreach (int i in reachable)
                        {
                            if (temp.endStates.Contains(i))
                            {
                                endState = true;
                                break;
                            }
                        }
                        newAddIndex = ret.addState(endState);
                        newStates.Add(reachable);
                        statesThatNeedProcessing.Enqueue(reachable);
                    }
                    if (interestingChars.Count > 0)
                    {
                        ret.connect(hasState, newAddIndex, new ConnectionAnythingBut(hasState, newAddIndex, interestingChars));
                    }
                    else
                    {
                        ret.connect(hasState, newAddIndex, new AnythingConnection(hasState, newAddIndex));
                    }
                }
            }
            int garbageState = ret.addState(false);
            ret.calculateConnectionPerState();
            for (int i = 1; i < ret.numStates; i++)
            {
                HashSet<char> unUsableConnections = new HashSet<char>();
                foreach (Connection c in ret.connectionPerState[i])
                {
                    unUsableConnections.UnionWith(c.acceptSet());
                }
                bool canUseAll = true;
                bool canUseAny = true;
                HashSet<char> usableConnections = new HashSet<char>();
                foreach (Connection c in ret.connectionPerState[i])
                {
                    if (c is AnythingConnection)
                    {
                        canUseAny = false;
                        break;
                    }
                    if (c is ConnectionAnythingBut)
                    {
                        if (canUseAll)
                        {
                            usableConnections = new HashSet<char>(((ConnectionAnythingBut)c).forbidden);
                            canUseAll = false;
                        }
                        else
                        {
                            usableConnections.IntersectWith(((ConnectionAnythingBut)c).forbidden);
                        }
                    }
                }
                if (canUseAny)
                {
                    if (!canUseAll)
                    {
                        usableConnections.ExceptWith(unUsableConnections);
                        if (usableConnections.Count > 0)
                        {
                            ret.connect(i, garbageState, new ConnectionAnyOf(i, garbageState, usableConnections));
                        }
                    }
                    else
                    {
                        if (unUsableConnections.Count > 0)
                        {
                            ret.connect(i, garbageState, new ConnectionAnythingBut(i, garbageState, unUsableConnections));
                        }
                        else
                        {
                            ret.connect(i, garbageState, new AnythingConnection(i, garbageState));
                        }
                    }
                }
            }
            ret.connect(garbageState, garbageState, new AnythingConnection(garbageState, garbageState));
            ret.calculateConnectionPerState();
            return ret;
        }
        public NFA complement()
        {
            return this.toDFA().not();
        }
        public NFA not()
        {
            NFA ret = new NFA(this);
            HashSet<int> newEndStates = new HashSet<int>();
            for (int i = 0; i < ret.numStates; i++)
            {
                if (!ret.endStates.Contains(i))
                {
                    newEndStates.Add(i);
                }
            }
            ret.endStates = newEndStates;
            return ret;
        }
        private void incrementIndices(int amount)
        {
            foreach (Connection c in connections)
            {
                c.increment(amount);
            }
            HashSet<int> newEndStates = new HashSet<int>();
            foreach (int i in endStates)
            {
                newEndStates.Add(i + amount);
            }
            endStates = newEndStates;
            startState += amount;
            numStates += amount;
            calculateConnectionPerState();
        }
        public NFA or(NFA second)
        {
            NFA ret = new NFA(this);
            //add states
            ret.incrementIndices(second.numStates);
            foreach (int i in second.endStates)
            {
                ret.endStates.Add(i);
            }
            //add connections between newly added states
            foreach (Connection c in second.connections)
            {
                ret.connections.Add(c.copy());
            }
            //connect new start states with old start state
            ret.connect(ret.startState, second.startState, new EpsilonConnection(-1, -1));
            ret.hasEpsilons = true;
            ret.calculateConnectionPerState();
            return ret;
        }
        private void addConnectionToEndStates(int destination, Connection connectionType)
        {
            foreach (int i in endStates)
            {
                connect(i, destination, connectionType);
            }
        }
        public NFA closure()
        {
            NFA ret = new NFA(this);
            //add new start state
            int oldStartState = ret.startState;
            int newStartState = ret.addState(false);
            ret.startState = newStartState;
            ret.connect(newStartState, oldStartState, new EpsilonConnection(0, 0));
            //add connections from old end states to old start state
            ret.addConnectionToEndStates(oldStartState, new EpsilonConnection(0, 0));
            //old start state is a valid end state
            ret.endStates.Add(newStartState);
            ret.hasEpsilons = true;
            return ret;
        }
        public NFA maybe()
        {
            NFA ret = new NFA(this);
            ret.endStates.Add(ret.startState);
            return ret;
        }
        public NFA conc(NFA second)
        {
            NFA ret = new NFA(this);
            //add new states
            ret.incrementIndices(second.numStates);
            foreach (Connection c in second.connections)
            {
                ret.connections.Add(c.copy());
            }
            ret.calculateConnectionPerState();
            //connect old end states to second's start state
            int secondStartState = second.startState;
            ret.addConnectionToEndStates(secondStartState, new EpsilonConnection(0, 0));
            //fix end states as second's end states
            ret.endStates = new HashSet<int>(second.endStates);
            ret.hasEpsilons = true;
            return ret;
        }
        public void deEpsilonate()
        {
            HashSet<Tuple<int, int>> epsilons = new HashSet<Tuple<int, int>>();
            foreach (Connection c in connections)
            {
                if (c is EpsilonConnection)
                {
                    epsilons.Add(new Tuple<int, int>(c.start, c.end));
                }
            }
            bool addedNew = true;
            while (addedNew)
            {
                addedNew = false;
                HashSet<Tuple<int, int>> newEpsilons = new HashSet<Tuple<int, int>>(epsilons);
                foreach (Tuple<int, int> a in epsilons)
                {
                    foreach (Tuple<int, int> b in epsilons)
                    {
                        if (a.Item2 == b.Item1)
                        {
                            if (newEpsilons.Add(new Tuple<int, int>(a.Item1, b.Item2)))
                            {
                                addedNew = true;
                            }
                        }
                    }
                }
                epsilons = newEpsilons;
            }
            foreach (Tuple<int, int> e in epsilons)
            {
                if (endStates.Contains(e.Item2))
                {
                    endStates.Add(e.Item1);
                }
                List<Connection> newConnections = new List<Connection>();
                foreach (Connection c2 in connectionPerState[e.Item2])
                {
                    if (!(c2 is EpsilonConnection))
                    {
                        Connection newConnection = c2.copy();
                        newConnection.start = e.Item1;
                        newConnections.Add(newConnection);
                    }
                }
                foreach(Connection c in newConnections)
                {
                    connect(c.start, c.end, c);
                }
            }
            for (int i = connections.Count - 1; i >= 0; i--)
            {
                if (connections[i] is EpsilonConnection)
                {
                    connections.RemoveAt(i);
                }
            }
            hasEpsilons = false;
            removeEmptyAnythingButs();
            removeDoubleConnections();
        }
        public void startRecognizing()
        {
            deEpsilonate();
            currentStates = new List<Tuple<int, int>>();
            currentStates.Add(new Tuple<int, int>(startState, 0));
        }
        public void alsoRecognizeFromHere()
        {
            currentStates.Add(new Tuple<int, int>(startState, 0));
        }
        public int recognizesCurrent()
        {
            foreach (Tuple<int, int> i in currentStates)
            {
                if (endStates.Contains(i.Item1))
                {
                    return i.Item2;
                }
            }
            return -1;
        }
        public void takeCharacter(char ch)
        {
            List<Tuple<int, int>> newStates = new List<Tuple<int, int>>();
            foreach (Tuple<int, int> s in currentStates)
            {
                foreach (Connection c in connectionPerState[s.Item1])
                {
                    if (c.accepts(ch))
                    {
                        newStates.Add(new Tuple<int, int>(c.end, s.Item2+1));
                    }
                }
            }
            currentStates = newStates;
        }
        public bool recognizes(String str)
        {
            startRecognizing();
            for (int i = 0; i < str.Length; i++)
            {
                takeCharacter(str[i]);
            }
            return recognizesCurrent() > -1;
        }
    }
}
