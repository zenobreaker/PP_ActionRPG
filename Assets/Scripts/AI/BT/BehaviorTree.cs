using AI.BT.Nodes;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AI.BT
{
    [CreateAssetMenu()]
    public class BehaviorTree : ScriptableObject
    {
        [SerializeField] protected  SO_Blackboard so_blackboard;
        protected SO_Blackboard blackboard;
        public SO_Blackboard Blackboard { get => blackboard; }

        public BTNode rootNode;
        public BTNode.NodeState state = BTNode.NodeState.Running;
        public List<BTNode> nodes = new List<BTNode>();


        public void CreateBlackboardKey()
        {

        }
        public RootNode CreateBehaviorTree()
        {
            return null; 
        }

        public void StopEvaluate()
        {
            rootNode?.StopEvaluate();
        }

        public void Evaluate()
        {
            rootNode?.Evaluate();
        }

        public BTNode CreateBTNode(System.Type type)
        {
            BTNode node = ScriptableObject.CreateInstance(type) as BTNode;
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();
            
            Undo.RecordObject(this, "Behavior Tree (CreateBTNode)");
            nodes.Add(node);

            AssetDatabase.AddObjectToAsset(node, this);
            Undo.RegisterCreatedObjectUndo(node, "Behavior Tree (CreateBTNode)");
            AssetDatabase.SaveAssets();
            return node; 
        }

        public void DeleteBTNode(BTNode node)
        {
            Undo.RecordObject(this, "Behavior Tree (CreateBTNode)");
            nodes.Remove(node);
            
            //AssetDatabase.RemoveObjectFromAsset(node);
            Undo.DestroyObjectImmediate(node);
            AssetDatabase.SaveAssets();
        }

        // 노드에 자식으로 추가하는 작업 
        public void AddChild(BTNode parent, BTNode child)
        {
            DecoratorNode decorator = parent as DecoratorNode;
            if(decorator != null)
            {
                Undo.RecordObject(decorator, "Behavior Tree (AddChild)");
                decorator.ChildNode = child;
                EditorUtility.SetDirty(decorator);
            }

            RootNode root = parent as RootNode;
            if (root != null)
            {
                Undo.RecordObject(root, "Behavior Tree (AddChild)");
                root.ChildNode = child;
                EditorUtility.SetDirty(root);
            }

            CompositeNode composite = parent as CompositeNode;
            if (composite != null)
            {
                Undo.RecordObject(composite, "Behavior Tree (AddChild)");
                composite.Children.Add(child);
                EditorUtility.SetDirty(composite);
            }
        }
        // 노드의 자식 제거 
        public void RemoveChild(BTNode parent, BTNode child)
        {
            DecoratorNode decorator = parent as DecoratorNode;
            if (decorator != null)
            {
                Undo.RecordObject(decorator, "Behavior Tree (RemoveChild)");
                decorator.ChildNode = null;
                EditorUtility.SetDirty(decorator);
            }

            RootNode root = parent as RootNode;
            if (root != null)
            {
                Undo.RecordObject(root, "Behavior Tree (RemoveChild)");
                root.ChildNode = null;
                EditorUtility.SetDirty(root);
            }

            CompositeNode composite = parent as CompositeNode;
            if (composite != null)
            {
                Undo.RecordObject(composite, "Behavior Tree (RemoveChild)");
                composite.Children.Remove(child);
                EditorUtility.SetDirty(composite);
            }
        }

        public List<BTNode> GetChildren(BTNode parent)
        {
            List<BTNode> childern = new List<BTNode>();

            DecoratorNode decorator = parent as DecoratorNode;
            if (decorator != null && decorator.ChildNode != null)
            {
                childern.Add(decorator.ChildNode);
            }

            RootNode root = parent as RootNode;
            if (root != null && root.ChildNode != null)
            {
                childern.Add(root.ChildNode);
            }

            CompositeNode composite = parent as CompositeNode;
            if (composite != null)
            {
                return composite.Children;
            }

            return childern;
        }

        public BehaviorTree Clone()
        {
            BehaviorTree tree = Instantiate(this);
            tree.rootNode = tree.rootNode.Clone();
            return tree; 
        }
    }
}
