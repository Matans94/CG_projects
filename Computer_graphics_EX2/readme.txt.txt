205710981 hadas241
204307664 matans



Why does the function "MakeFlatShaded" works ? 

Before adding the vertexes, each vertex holds the information as an average of the surfaces around it. 
Afterwards, we created a unique vertex for each surface, 
this vertex is now holding only the normal of one surface (instead of the average of multiple surfaces).
So now, when we will look at a spot in the world , 
because there are multiple vertexes at the same spot (one per surface), and each vertex points to a different direction, 
witch is the direction of the normal of the surface it belongs. 
This process gives us the feeling that from one point there are number of surfaces. 
First, while we look at one surface, it’s three vertexes has a different normal value. 
This difference creates gradation of the color. After using the method, 
when we look at one surface its three vertexes have the same normal value. 
That is why the color is uniform and we get the “flat shading” effect. 

לפני הוספת הקודקודים, כל קודקוד ברשימה שקיבלנו החזיק את המידע באופן ממוצע של כל המשטחים סביבו. 
לאחר מכן, לאחר ייחוד כל קודקוד למשטח אחד, 
הקודקוד מכיל רק את הנורמל של אותו משטח. כך כאשר נסתכל על נקודה בעולם, 
קיימים כמה קודקודים באותה הנקודה (כמספר הפאות סביבה), 
שכל אחד מהקודקודים מצביע לכיוון אחר שהוא הנורמל של הפאה אליו שייך. 
זה מה שנותן לנו את התחושה של מנקודה אחת יוצאים כמה פאות ואת החלוקה למשולשים. 

בהתחלה, כאשר נסתכל על פאה אחת, 
שלושת הקודקודים שלה מחזיקים נורמלים שונים והאינטרפולציה שמתבצעת גורמת להדרגתיות בצבע. לאחר הפעלת המתודה, 
כאשר נסתכל על פאה שלושת הקודקודים שלה מצביעים על אותו נורמל,
 ולכן הצבע במשולש הוא אחיד וכך נוצר אפקט  “flat shading”  .
