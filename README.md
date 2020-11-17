## BEV range management in VR

For my master's thesis I developed a driving simulator in VR to test two electric vehicle information system (EVIS), commonly know as dashboards.  

The given task is to travel the entire distance without running out of battery. Starting SoC is 1.2 kwh.

The scenario is setup as a single route road without other vehicles, pedestrians, stop signs or traffic lights. The first road section has a speed limit of 40 km/h and the latter half is limited to 110 km/h. The transition from 40 to 110 km/h can cause drastic changes in the range estimation displayed by a conventional EVIS. Hence it's interesting to see if drivers adapt to the situation differently when using a novel EVIS utilizing transparency regarding the range estimate.

The simulator is developed using Unity where I've implemented a drivable vehicle with a battery model based on Newton's second law. During the drive data is collected about the speed, SoC and so forth. To allow for later analysis of driving behaviors.

The analysis was previously done using Matlab but has later been transferred over to Python and Jupyter notebooks.

Here are two videos showing what it looks like whilst driving using both EVIS.
Conventional EVIS: https://youtu.be/T2p7Rt-5WcI
Novel EVIS: https://youtu.be/_AtB8SAgGx0

Thesis: https://www.diva-portal.org/smash/record.jsf?pid=diva2:1469206