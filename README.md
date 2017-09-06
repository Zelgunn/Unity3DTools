# Unity3DTools
Some tools for Unity3D

- SpeechListener :
  Use to recognize a keyword an call an event (works like events used for GUI).
  Place SpeechListenerEditor in a folder called "Editor".

- Triggers Editor:
  An editor inspired by the Triggers editor in Warcraft III.
  "Triggers" are made of 3 types of components:
    1) Events : When one of them occurs, the trigger starts
	2) Conditions : When a triggers starts, it will stops if one or more of the conditions is false
	3) Actions : If all conditions are met, the trigger will do its actions
  
  Events, conditions and actions are made of nodes, for an intuitive and easy customization.
  If you want to make your own components, just use the attribute [NodeMethod] before your functions and they will appear in the editor.
  Components belong to categories (specified in the attribute), if you want to use your own categories, create a Node Category asset (with the same name).