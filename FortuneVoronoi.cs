using System;
using System.Collections;

using Microsoft.Xna.Framework;
using SteeleSky.Voronoi.Data;
using SteeleSky.Voronoi.Mathematics;

namespace SteeleSky.Voronoi
{
	public class VoronoiGraph
	{
		public HashSet Vertizes = new HashSet();
		public HashSet Edges = new HashSet();
	}
	public class VoronoiEdge
	{
		public Vector2 RightData, LeftData;
		public Vector2 VVertexA = Fortune.VVUnkown, VVertexB = Fortune.VVUnkown;
		public void AddVertex(Vector2 V)
		{
			if(VVertexA==Fortune.VVUnkown)
				VVertexA = V;
			else if(VVertexB==Fortune.VVUnkown)
				VVertexB = V;
			else throw new Exception("Tried to add third vertex!");
		}
	}
	
	// VoronoiVertex or VoronoiDataPoint are represented as Vector2

	internal abstract class VNode
	{
		private VNode _Parent = null;
		private VNode _Left = null, _Right = null;
		public VNode Left
		{
			get{return _Left;}
			set
			{
				_Left = value;
				value.Parent = this;
			}
		}
		public VNode Right
		{
			get{return _Right;}
			set
			{
				_Right = value;
				value.Parent = this;
			}
		}
		public VNode Parent
		{
			get{return _Parent;}
			set{_Parent = value;}
		}


		public void Replace(VNode ChildOld, VNode ChildNew)
		{
			if(Left==ChildOld)
				Left = ChildNew;
			else if(Right==ChildOld)
				Right = ChildNew;
			else throw new Exception("Child not found!");
			ChildOld.Parent = null;
		}

		public static VDataNode FirstDataNode(VNode Root)
		{
			VNode C = Root;
			while(C.Left!=null)
				C = C.Left;
			return (VDataNode)C;
		}
		public static VDataNode LeftDataNode(VDataNode Current)
		{
			VNode C = Current;
			//1. Up
			do
			{
				if(C.Parent==null)
					return null;
				if(C.Parent.Left == C)
				{
					C = C.Parent;
					continue;
				}
				else
				{
					C = C.Parent;
					break;
				}
			}while(true);
			//2. One Left
			C = C.Left;
			//3. Down
			while(C.Right!=null)
				C = C.Right;
			return (VDataNode)C; // Cast statt 'as' damit eine Exception kommt
		}
		public static VDataNode RightDataNode(VDataNode Current)
		{
			VNode C = Current;
			//1. Up
			do
			{
				if(C.Parent==null)
					return null;
				if(C.Parent.Right == C)
				{
					C = C.Parent;
					continue;
				}
				else
				{
					C = C.Parent;
					break;
				}
			}while(true);
			//2. One Right
			C = C.Right;
			//3. Down
			while(C.Left!=null)
				C = C.Left;
			return (VDataNode)C; // Cast statt 'as' damit eine Exception kommt
		}

		public static VEdgeNode EdgeToRightDataNode(VDataNode Current)
		{
			VNode C = Current;
			//1. Up
			do
			{
				if(C.Parent==null)
					throw new Exception("No Left Leaf found!");
				if(C.Parent.Right == C)
				{
					C = C.Parent;
					continue;
				}
				else
				{
					C = C.Parent;
					break;
				}
			}while(true);
			return (VEdgeNode)C;
		}

		public static VDataNode FindDataNode(VNode Root, float ys, float x)
		{
			VNode C = Root;
			do
			{
				if(C is VDataNode)
					return (VDataNode)C;
				if(((VEdgeNode)C).Cut(ys,x)<0)
					C = C.Left;
				else
					C = C.Right;
			}while(true);
		}

		/// <summary>
		/// Will return the new root (unchanged except in start-up)
		/// </summary>
		public static VNode ProcessDataEvent(VDataEvent e, VNode Root, VoronoiGraph VG, float ys, out VDataNode[] CircleCheckList)
		{
			if(Root==null)
			{
				Root = new VDataNode(e.DataPoint);
				CircleCheckList = new VDataNode[] {(VDataNode)Root};
				return Root;
			}
			//1. Find the node to be replaced
			VNode C = VNode.FindDataNode(Root, ys, e.DataPoint.X);
			//2. Create the subtree (ONE Edge, but two VEdgeNodes)
			VoronoiEdge VE = new VoronoiEdge();
			VE.LeftData = ((VDataNode)C).DataPoint;
			VE.RightData = e.DataPoint;
			VE.VVertexA = Fortune.VVUnkown;
			VE.VVertexB = Fortune.VVUnkown;
			VG.Edges.Add(VE);

			VNode SubRoot;
			if(Math.Abs(VE.LeftData.Y-VE.RightData.Y)<1e-10)
			{
				if(VE.LeftData.X<VE.RightData.X)
				{
					SubRoot = new VEdgeNode(VE,false);
					SubRoot.Left = new VDataNode(VE.LeftData);
					SubRoot.Right = new VDataNode(VE.RightData);
				}
				else
				{
					SubRoot = new VEdgeNode(VE,true);
					SubRoot.Left = new VDataNode(VE.RightData);
					SubRoot.Right = new VDataNode(VE.LeftData);
				}
				CircleCheckList = new VDataNode[] {(VDataNode)SubRoot.Left,(VDataNode)SubRoot.Right};
			}
			else
			{
				SubRoot = new VEdgeNode(VE,false);
				SubRoot.Left = new VDataNode(VE.LeftData);
				SubRoot.Right = new VEdgeNode(VE,true);
				SubRoot.Right.Left = new VDataNode(VE.RightData);
				SubRoot.Right.Right = new VDataNode(VE.LeftData);
				CircleCheckList = new VDataNode[] {(VDataNode)SubRoot.Left,(VDataNode)SubRoot.Right.Left,(VDataNode)SubRoot.Right.Right};
			}

			//3. Apply subtree
			if(C.Parent == null)
				return SubRoot;
			C.Parent.Replace(C,SubRoot);
			return Root;
		}
		public static VNode ProcessCircleEvent(VCircleEvent e, VNode Root, VoronoiGraph VG, float ys, out VDataNode[] CircleCheckList)
		{
			VDataNode a,b,c;
			VEdgeNode eu,eo;
			b = e.NodeN;
			a = VNode.LeftDataNode(b);
			c = VNode.RightDataNode(b);
			if(a==null || b.Parent==null || c==null || !a.DataPoint.Equals(e.NodeL.DataPoint) || !c.DataPoint.Equals(e.NodeR.DataPoint))
			{
				CircleCheckList = new VDataNode[]{};
				return Root; // Abbruch da sich der Graph verändert hat
			}
			eu = (VEdgeNode)b.Parent;
			CircleCheckList = new VDataNode[] {a,c};
			//1. Create the new Vertex
			Vector2 VNew = new Vector2(e.Center.X,e.Center.Y);
//			VNew[0] = Fortune.ParabolicCut(a.DataPoint[0],a.DataPoint[1],c.DataPoint[0],c.DataPoint[1],ys);
//			VNew[1] = (ys + a.DataPoint[1])/2 - 1/(2*(ys-a.DataPoint[1]))*(VNew[0]-a.DataPoint[0])*(VNew[0]-a.DataPoint[0]);
			VG.Vertizes.Add(VNew);
			//2. Find out if a or c are in a distand part of the tree (the other is then b's sibling) and assign the new vertex
			if(eu.Left==b) // c is sibling
			{
				eo = VNode.EdgeToRightDataNode(a);

				// replace eu by eu's Right
				eu.Parent.Replace(eu,eu.Right);
			}
			else // a is sibling
			{
				eo = VNode.EdgeToRightDataNode(b);

				// replace eu by eu's Left
				eu.Parent.Replace(eu,eu.Left);
			}
			eu.Edge.AddVertex(VNew);
//			///////////////////// uncertain
//			if(eo==eu)
//				return Root;
//			/////////////////////
			eo.Edge.AddVertex(VNew);
			//2. Replace eo by new Edge
			VoronoiEdge VE = new VoronoiEdge();
			VE.LeftData = a.DataPoint;
			VE.RightData = c.DataPoint;
			VE.AddVertex(VNew);
			VG.Edges.Add(VE);

			VEdgeNode VEN = new VEdgeNode(VE, false);
			VEN.Left = eo.Left;
			VEN.Right = eo.Right;
			if(eo.Parent == null)
				return VEN;
			eo.Parent.Replace(eo,VEN);
			return Root;
		}
		public static VCircleEvent CircleCheckDataNode(VDataNode n, float ys)
		{
			VDataNode l = VNode.LeftDataNode(n);
			VDataNode r = VNode.RightDataNode(n);
			if(l==null || r==null || l.DataPoint==r.DataPoint || l.DataPoint==n.DataPoint || n.DataPoint==r.DataPoint)
				return null;
			if(MathTools.ccw(l.DataPoint,n.DataPoint,r.DataPoint,false)<=0)
				return null;
			Vector2 Center = Fortune.CircumCircleCenter(l.DataPoint,n.DataPoint,r.DataPoint);
			VCircleEvent VC = new VCircleEvent();
			VC.NodeN = n;
			VC.NodeL = l;
			VC.NodeR = r;
			VC.Center = Center;
			VC.Valid = true;
			if(VC.Y>=ys)
				return VC;
			return null;
		}
	}

	internal class VDataNode : VNode
	{
		public VDataNode(Vector2 DP)
		{
			this.DataPoint = DP;
		}
		public Vector2 DataPoint;
	}

	internal class VEdgeNode : VNode
	{
		public VEdgeNode(VoronoiEdge E, bool Flipped)
		{
			this.Edge = E;
			this.Flipped = Flipped;
		}
		public VoronoiEdge Edge;
		public bool Flipped;
		public float Cut(float ys, float x)
		{
			if(!Flipped)
				return (float)Math.Round(x-Fortune.ParabolicCut(Edge.LeftData.X, Edge.LeftData.Y, Edge.RightData.X, Edge.RightData.Y, ys),10);
			return (float)Math.Round(x-Fortune.ParabolicCut(Edge.RightData.X, Edge.RightData.Y, Edge.LeftData.X, Edge.LeftData.Y, ys),10);
		}
	}


	internal abstract class VEvent : IComparable
	{
		public abstract float Y {get;}
		public abstract float X {get;}
		#region IComparable Members

		public int CompareTo(object obj)
		{
			if(!(obj is VEvent))
				throw new ArgumentException("obj not VEvent!");
			int i = Y.CompareTo(((VEvent)obj).Y);
			if(i!=0)
				return i;
			return X.CompareTo(((VEvent)obj).X);
		}

		#endregion
	}

	internal class VDataEvent : VEvent
	{
		public Vector2 DataPoint;
		public VDataEvent(Vector2 DP)
		{
			this.DataPoint = DP;
		}
		public override float Y
		{
			get
			{
				return DataPoint.Y;
			}
		}

		public override float X
		{
			get
			{
				return DataPoint.X;
			}
		}

	}

	internal class VCircleEvent : VEvent
	{
		public VDataNode NodeN, NodeL, NodeR;
		public Vector2 Center;
		public override float Y
		{
			get
			{

                return (float)Math.Round(Center.Y + Vector2.Distance(NodeN.DataPoint, Center), 10);
			}
		}

		public override float X
		{
			get
			{
				return Center.X;
			}
		}

		public bool Valid = true;
	}

	public abstract class Fortune
	{
        public static readonly Vector2 VVInfinite = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        public static readonly Vector2 VVUnkown = new Vector2(float.NaN, float.NaN);
        
		internal static float ParabolicCut(float x1, float y1, float x2, float y2, float ys)
		{
//			y1=-y1;
//			y2=-y2;
//			ys=-ys;
//			
			if(Math.Abs(x1-x2)<1e-10 && Math.Abs(y1-y2)<1e-10)
			{
//				if(y1>y2)
//					return float.PositiveInfinity;
//				if(y1<y2)
//					return float.NegativeInfinity;
//				return x;
				throw new Exception("Identical datapoints are not allowed!");
			}

			if(Math.Abs(y1-ys)<1e-10 && Math.Abs(y2-ys)<1e-10)
				return (x1+x2)/2;
			if(Math.Abs(y1-ys)<1e-10)
				return x1;
			if(Math.Abs(y2-ys)<1e-10)
				return x2;
			float a1 = 1/(2*(y1-ys));
			float a2 = 1/(2*(y2-ys));
			if(Math.Abs(a1-a2)<1e-10)
				return (x1+x2)/2;
			float xs1 = 0.5f/(2*a1-2*a2)*(4*a1*x1-4*a2*x2+2*(float)Math.Sqrt(-8*a1*x1*a2*x2-2*a1*y1+2*a1*y2+4*a1*a2*x2*x2+2*a2*y1+4*a2*a1*x1*x1-2*a2*y2));
			float xs2 = 0.5f/(2*a1-2*a2)*(4*a1*x1-4*a2*x2-2* (float)Math.Sqrt(-8*a1*x1*a2*x2-2*a1*y1+2*a1*y2+4*a1*a2*x2*x2+2*a2*y1+4*a2*a1*x1*x1-2*a2*y2));
			xs1=(float)Math.Round(xs1,10);
			xs2=(float)Math.Round(xs2,10);
			if(xs1>xs2)
			{
				float h = xs1;
				xs1=xs2;
				xs2=h;
			}
			if(y1>=y2)
				return xs2;
			return xs1;
		}
		internal static Vector2 CircumCircleCenter(Vector2 A, Vector2 B, Vector2 C)
		{
			if(A==B || B==C || A==C)
				throw new Exception("Need three different points!");
			float tx = (A.X + C.X)/2;
			float ty = (A.Y + C.Y)/2;

            float vx = (B.X + C.X) / 2;
            float vy = (B.Y + C.Y) / 2;

            float ux, uy, wx, wy;
			
			if(A.X == C.X)
			{
				ux = 1;
				uy = 0;
			}
			else
			{
				ux = (C.Y - A.Y)/(A.X - C.X);
				uy = 1;
			}

			if(B.X == C.X)
			{
				wx = -1;
				wy = 0;
			}
			else
			{
				wx = (B.Y - C.Y)/(B.X - C.X);
				wy = -1;
			}

			float alpha = (wy*(vx-tx)-wx*(vy - ty))/(ux*wy-wx*uy);

			return new Vector2((float)(tx+alpha*ux),(float)ty+alpha*uy);
		}	
		public static VoronoiGraph GenerateGraph(IEnumerable Datapoints)
		{
			BinaryPriorityQueue PQ = new BinaryPriorityQueue();
			Hashtable CurrentCircles = new Hashtable();
			VoronoiGraph VG = new VoronoiGraph();
			VNode RootNode = null;
			foreach(Vector2 V in Datapoints)
			{
				PQ.Push(new VDataEvent(V));
			}
			while(PQ.Count>0)
			{
				VEvent VE = PQ.Pop() as VEvent;
				VDataNode[] CircleCheckList;
				if(VE is VDataEvent)
				{
					RootNode = VNode.ProcessDataEvent(VE as VDataEvent,RootNode,VG,VE.Y,out CircleCheckList);
				}
				else if(VE is VCircleEvent)
				{
					CurrentCircles.Remove(((VCircleEvent)VE).NodeN);
					if(!((VCircleEvent)VE).Valid)
						continue;
					RootNode = VNode.ProcessCircleEvent(VE as VCircleEvent,RootNode,VG,VE.Y,out CircleCheckList);
				}
				else throw new Exception("Got event of type "+VE.GetType().ToString()+"!");
				foreach(VDataNode VD in CircleCheckList)
				{
					if(CurrentCircles.ContainsKey(VD))
					{
						((VCircleEvent)CurrentCircles[VD]).Valid=false;
						CurrentCircles.Remove(VD);
					}
					VCircleEvent VCE = VNode.CircleCheckDataNode(VD,VE.Y);
					if(VCE!=null)
					{
						PQ.Push(VCE);
						CurrentCircles[VD]=VCE;
					}
				}
				if(VE is VDataEvent)
				{
					Vector2 DP = ((VDataEvent)VE).DataPoint;
					foreach(VCircleEvent VCE in CurrentCircles.Values)
					{

                        if (Vector2.Distance(DP, VCE.Center) < VCE.Y - VCE.Center.Y && Math.Abs(Vector2.Distance(DP,VCE.Center) - (VCE.Y - VCE.Center.Y)) > 1e-10)
							VCE.Valid = false;
					}
				}
			}
			return VG;
		}
	}
}
