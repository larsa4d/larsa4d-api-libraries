import win32com.client

def CreateObject(class_name):
    return win32com.client.Dispatch(class_name)

project = CreateObject("LarsaElements.clsProject")

joint1 = CreateObject("LarsaElements.clsJoint")
joint1.location.setCoordinates(0, 0, 0)
project.joints.append(joint1)

joint2 = CreateObject("LarsaElements.clsJoint")
joint2.location.setCoordinates(10, 0, 0)
project.joints.append(joint2)

member = CreateObject("LarsaElements.clsMember")
member.Setjoint(1, joint1)
member.Setjoint(2, joint2)
project.members.append(member)

lc = CreateObject("LarsaElements.clsLoadCase")
project.primaryLoadCases.append(lc)
mload = CreateObject("LarsaElements.clsMemberLoad")
lc.MemberLoads.append(mload)
mload.member = member
mload.loadType = win32com.client.constants.UniformForce
mload.startW = 10

filename = "test.lar"
lar_format_version = (8, 1, 14)
project.SaveToFile(filename, win32com.client.constants.FILE_FORMAT_LAR6A,
                   *lar_format_version, None)
