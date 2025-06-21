# Rules

---

##  Object Template

*  ### Description
   
   Describes only contracts that object should have like class in pure c#. Description should be in declarative style. In complicated cases it can be imperative. Support nesting from other templates and other opertion with sets of contracts.
   

* ### Rules
   
  - Prefer Declarative style.
  - Should not contains any information about how to resolve.
  - In complicated cases possible to configure instances manualy.

---

##  Object

*  ### Description

   **Object** - set of contracts like many interfaces for one class in pure c#. Provides api for getting services of object. In context of ObjectCompose can be considered as state. Has its own identity.
   

* ### Rules
   
   - Should provides services if has, otherwise throw exception.

---

##  Object Builder

*  ### Description
   ```
   Builder for object. Provides api for configuring services of object like 
   set them up for a specific instance. Nesting the other templates for runtime
   configuration and othe sets operations.
   ```

* ### Rules
   ```
   Should provides services if has, otherwise throw exception.
   ```



