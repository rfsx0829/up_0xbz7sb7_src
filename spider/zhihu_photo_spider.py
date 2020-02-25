import re
import os
import json
from urllib import request

def main():
    answers = getAnswers()
    allCount = 0
    for each in answers:
        links = answers[each]
        for singleAnswer in links:
            imgs = findImgs(singleAnswer)
            if imgs.__len__() == 0:
                continue
            successCount = saveImgs(each, allCount, imgs)
            allCount += successCount
            print("OK  ... %s ... %d" % (each, successCount))

def saveImgs(folder, offset, links):
    try:
        os.mkdir(folder)
    except:
        pass

    count = 0
    for each in links:
        try:
            data = request.urlopen(each).read()
            f = open("%s/%d.jpg" % (folder, count+offset), "wb+")
            f.write(data)
            f.close()
            count += 1
        except:
            continue
    return count

def findImgs(singleAnswer):
    try:
        data = request.urlopen(singleAnswer).read()
        content = re.findall(r"(<span class=\"RichText ztext CopyrightRichText-richText\".*?</span>)", str(data, encoding="utf8"))[0]
        return re.findall(r"<img src=\"(https://.*?.jpg)\"", content)
    except:
        return []

def getAnswers():
    content = getContent()

    answers = {}
    current = ""

    labels = re.findall(r"<(.*?)>(.*?)</.*?>", content)
    for each in labels:
        try:
            if each[0][0] == 'p' and each[1].index("cup") > 0:
                cup = re.findall(r"(.*?) *cup", each[1])[0]
                current = cup
            elif each[0][0] == 'a':
                if answers.__contains__(current) == False:
                    answers[current] = []
                link = re.findall(r"a href=\"(.*?)\"", each[0])[0]
                answers[current].append(link)
        except:
            continue
    return answers

def getContent():
    url = "https://www.zhihu.com/api/v4/questions/58498720/answers?include=content&limit=1&offset=0"
    data = request.urlopen(url)
    obj = json.load(data)
    content = obj["data"][0]["content"]
    return content

if __name__ == "__main__":
    main()
    input("press any key")
