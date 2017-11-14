# DraggableTreeViewTest
A simple WPF project for a draggable, data-binding treeview.
This testing app contains a working window, and some fake data to form a tree view.

## Features
1. It has a tree node class `MyNode`, which is data-bound to the WPF view.
2. The WPF TreeView could be drag and drop, with event fired on tree node when dragged over/dropped.
3. The `StretchingTreeView` class could stretch to the entire space of a tree node.
4. While expanding a tree node, the previous expanded tree node/bracch will be collapsed if the new node is not on the branch.
5. Cut and paste for a tree node or a tree branch.
