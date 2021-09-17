;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;;
;;; Zápočtová úloha 2
;;;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

#|

Nutné načíst soubory:

 load.lisp (knihovna micro-graphics)
 08_text-shape.lisp
 08-extended.lisp (vlastní rozšířená verze zdrojového kódu k 8. přednášce)

|#

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

#|

Dokumentace


Prohlížeč zobrazuje vlastnosti o zvoleném objektu v podle seznamu jeho
vlastností. Seznam vlastností zvoleného objektu zjistí inspektor posláním
zprávy list-of-properties tomuto objektu. Objekt v reakci na tuto zprávu
vrátí seznam svých vlastností. Metoda list-of-properties, která vrací seznam
vlastností daného objektu, je definována u třídy shape a abstract-window.


Nové vlastnosti potomků shape a abstract-window

U tříd, které jsou potomky shape nebo abstract-window, přidáme nové vlastnosti
do prohlížeče přepsáním metody list-of-properties přímo v uživatelské třídě.
V této metodě spojíme existujícího seznamu vlastností předka se seznamem nových
vlastností uživatelské třídy.
Příklad:
(defmethod list-of-properties ((class class))
  (append (call-next-method) (list 'prop1 'prop2 ... 'propn)))


Nové vlastnosti jiných tříd

Pokud uživatelská třída není potomkem třídy shape ani abstract-window, pak
je nutné této třídě nadefinovat novou metodu list-of-properties, která bude
vracet seznam vlastností této třídy, které bude prohlížeč schopen zobrazovat.
Příklad:
(defmethod list-of-properties ((class class))
  (list 'prop1 'prop2 ... 'propn))

|#

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;;
;;; Třída inspected-window
;;;

(defclass inspected-window (window)
  ())

(defmethod do-set-shape ((w inspected-window) s)
  (call-next-method)
  (send-event w 'ev-inspected-window-changed)
  w)

(defmethod set-background ((w inspected-window) color)
  (call-next-method)
  (send-event w 'ev-inspected-window-changed)
  w)

(defmethod mouse-down-inside-shape ((w inspected-window) shape button position)
  (call-next-method)
  (send-event w 'ev-mouse-down-inside-shape shape)
  w)

(defmethod mouse-down-no-shape ((w inspected-window) button position)
  (call-next-method)
  (send-event w 'ev-mouse-down-no-shape)
  w)

(defmethod ev-change ((w inspected-window) sender)
  (call-next-method)
  (send-event w 'ev-inspected-window-changed sender))



;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;;
;;; Třída inspector-window
;;;

(defclass inspector-window (abstract-window)
  ((inspected-window :initform nil)
   (inspected-object :initform nil)))

(defmethod inspected-window ((w inspector-window))
  (slot-value w 'inspected-window))

(defmethod set-inspected-window ((w inspector-window) value)
  (unless (typep value 'inspected-window)
    (error "Inspector-window accepts only inspected-window type"))
  (do-set-inspected-window w value))

(defmethod do-set-inspected-window ((w inspector-window) value)
  (setf (slot-value w 'inspected-window) value)
  (set-delegate value w)
  (add-events-to-inspected-window value)
  (make-window-info w value)
  w)

(defun add-events-to-inspected-window (value)
  (let ((events '(ev-inspected-window-changed ev-mouse-down-inside-shape ev-mouse-down-no-shape)))
    (dolist (event events)
      (add-event value event event))))

(defmethod inspected-object ((w inspector-window))
  (slot-value w 'inspected-object))

(defmethod do-set-inspected-object ((w inspector-window) object)
  (setf (slot-value w 'inspected-object) object))


#|
Události přijímané od inspected-window
|#

(defmethod ev-inspected-window-changed ((w inspector-window) sender &optional changed-object)
  (if changed-object
      (progn (when (inspected-object w)
               (make-window-info w (inspected-object w))))
    (progn (unless (inspected-object w)
             (make-window-info w sender)))))

(defmethod ev-mouse-down-inside-shape ((w inspector-window) sender object)
  (do-set-inspected-object w object)
  (make-window-info w object))

(defmethod ev-mouse-down-no-shape ((w inspector-window) sender)
  (do-set-inspected-object w nil)
  (make-window-info w sender))


#|
Instalace callbacku :double-click, metod a událostí, které s dvojklikem pracují
|#

(defmethod install-callbacks ((w inspector-window))
  (call-next-method)
  (install-double-click-callback w))

(defmethod install-double-click-callback ((w inspector-window))
  (mg:set-callback 
   (slot-value w 'mg-window) 
   :double-click (lambda (mgw button x y)
                   (declare (ignore mgw))
                   (window-double-click 
                    w
                    button 
                    (move (make-instance 'point) x y))))
  w)


(defmethod window-double-click ((w inspector-window) button position)
  (let ((shape (find-clicked-shape w position)))
    (if shape
        (double-click-inside-shape w shape button position)
      (double-click-no-shape w button position))))

(defmethod double-click-inside-shape ((w inspector-window) shape button position)
  (when (typep shape 'text-picture)
    (let ((list (multiple-value-list
                   (capi:prompt-for-value "Zadejte novou hodnotu"))))
      (when (second list)
        (double-click w shape (first list)))))
  w)

(defmethod double-click-no-shape ((w inspector-window) button position)
  w)

(defmethod double-click ((w inspector-window) shape value)
  (send-event shape 'ev-double-click shape value))

(defmethod ev-double-click ((w inspector-window) sender clicked value)
  (let* ((property (property clicked))
         (set-function (setter-name (property clicked))))
    (if (or (eql property 'shape) (eql property 'background))
        (funcall set-function (inspected-window w) value)
      (progn (funcall set-function (inspected-object w) value)
        (make-window-info w (inspected-object w))))))


#|
Metody pro zobrazování informací o inspected-window a inspected-object
|#

(defmethod make-window-info ((w inspector-window) value)
  (let* ((pic (make-instance 'picture))
         (items (do-make-window-info w value (list-of-properties value)))
         (text-pics (third items)))
    (set-items pic (apply 'append items))
    (set-delegates w text-pics)
    (do-set-shape w pic)
    (invalidate w)))

(defmethod do-make-window-info ((w inspector-window) value texts)
  (let ((text-class (set-text (make-instance 'text-shape)
                              (format nil "CLASS: ~a" (type-of value))))
        (text-list (make-text-list texts))
        (text-pics (make-text-pics (length texts))))
    (set-text-pics texts text-pics value)
    (move-texts text-class text-list text-pics)
    (list (list text-class) text-list text-pics)))


#|
Pomocné metody a funkce pro zobrazování informací
|#

(defmethod set-delegates ((w inspector-window) list)
  (dolist (item list)
    (set-delegate item w)))

(defun make-text (text)
  (set-text (make-instance 'text-shape) (format nil "~a:" text)))

(defun make-text-picture ()
  (make-instance 'text-picture))

(defun make-text-list (texts)
  (let ((list '()))
    (dolist (text texts)
      (setf list (append list (list (make-text text)))))
    list))

(defun make-text-pics (num)
  (let ((list '()))
    (dotimes (x num)
      (setf list (append list (list (make-text-picture)))))
    list))

(defun move-texts (text text-list-1 text-list-2)
  (move text 20 30)
  (move-text-list text text-list-1)
  (move-text-pics text-list-1 text-list-2))

(defun move-text-list (def-text list)
  (dotimes (x (length list))
    (move (nth x list) (left def-text) (+ (+ (top def-text) 11) (* (+ x 1) 16)))))

(defun move-text-pics (def-list list)
  (dotimes (x (length list))
    (move (nth x list) (+ (right (nth x def-list)) (correct-x-move-text (nth x def-list)))
          (- (top (nth x def-list)) (top (nth x list))))))

(defun correct-x-move-text (text)
  (/ (- (right text) (left text)) 3))

(defun set-text-pics (texts text-pics value)
  (dotimes (x (length text-pics))
    (set-text (nth x text-pics) (format nil "~a" (funcall (nth x texts) value))))
  (set-text-pics-properties texts text-pics)
  (set-text-pics-events text-pics))

(defun set-text-pics-properties (texts text-pics)
  (dotimes (x (length texts))
    (set-property (nth x text-pics) (nth x texts))))

(defun set-text-pics-events (text-pics)
  (dolist (item text-pics)
    (add-event item 'ev-double-click 'ev-double-click)))

(defun setter-name (prop)
  (values (find-symbol (format nil "SET-~a" prop))))



;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;;
;;; Třída text-picture
;;; 

#|
Pomocná třída pro vytvoření textu v obrázku pro lepší klikání
Podobné třídě button
|#

(defclass text-picture (abstract-picture)
  ((property :initform nil)))

(defmethod initialize-instance ((tp text-picture) &key)
  (call-next-method)
  (do-set-items tp (list (make-instance 'text-shape)))
  tp)

(defmethod text ((tp text-picture))
  (first (items tp)))

(defmethod set-text ((tp text-picture) value)
  (set-text (text tp) value)
  tp)

(defmethod top ((tp text-picture))
  (top (text tp)))

(defmethod solidp ((tp text-picture))
  t)

(defmethod property ((tp text-picture))
  (slot-value tp 'property))

(defmethod set-property ((tp text-picture) value)
  (setf (slot-value tp 'property) value))


