## TO DO

General
- [ ] write sensible readme
- [ ] restrict access modifiers

View
- [ ] residential/commercial demand chart
- [X] visualise usage amount/growth
- [ ] land/water tile drawing

Control
- [ ] input abstraction (to handle multiple modalities: mouse, touch, lego, ...)
- [X] rectangle zoning
  - [X] mouse
  - [X] touch
  - [X] draw rectangle
- [ ] line zoning (for roads)
- [ ] time
  - [X] pause/continue
    - [ ] pause/continue UI element
  - [ ] faster/slower
- [X] touch camera movement (2-finger-drag)
- [X] main menu
  - [ ] touch: toggle by flick
  - [X] anim: slide in from top of screen

Model
- [ ] land/water tile property
  - [ ] get from map poly
    - [ ] poly bounds checking
  - [ ] bridges
- [X] keep time (ticks)
- [ ] money
  - [X] building/zoning cost per tile
  - [ ] maintenence cost per tick
  - [ ] tax income
  - [ ] tax settings window
- [ ] traffic
  - [ ] routing between jobs/pop
  - [ ] road connectivity
- [ ] growth
  - [X] grow usage
  - [ ] keep track of total population/needs
  - [ ] extrinsic job/pop growth?
  - [ ] exponential pop growth? (i.e. births)
  - [ ] grow jobs by zoning?
  - [ ] growth distribution more interesting (e.g. more growth with higher diversity/connectivity, ...), general attractiveness score for tiles
- [ ] anti-growth
  - [ ] decrease in job/pop, when not fulfilling demands
  - [ ] abandonment/desolation of unused buildings
- [ ] multi-tile structures (e.g. public offices, but also for variability in res/comm)

Assets
- [ ] bridges
- [X] residential graphics
  - [X] growth stadii
  - [ ] diversity
- [X] commercial graphics
  - [X] growth stadii
  - [ ] diversity
- [X] traffic
  - [ ] density