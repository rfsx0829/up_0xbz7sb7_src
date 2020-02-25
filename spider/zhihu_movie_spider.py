from urllib import request
import json
import re

API = "https://www.zhihu.com/api/v4/questions/25699277/answers?include=content"
allfilms = {}

def main():
    url = API
    count = 50
    while count > 0:
        filmlist, nexturl = url2films(url)
        url = nexturl
        count -= 1
        for each in filmlist:
            if allfilms.__contains__(each) == False:
                allfilms[each] = 0
            allfilms[each] += 1
    sorteddict = sorted(allfilms.items(), key=lambda d:d[1], reverse=True)
    for each in sorteddict:
        if each[1] < 5:
            continue
        print(each)

def url2films(url):
    filmlist = []
    data = request.urlopen(url).read()
    obj = json.loads(data)
    for each in obj["data"]:
        films = findfilms(each["content"])
        for film in films:
            filmlist.append(film)
    return (filmlist, obj["paging"]["next"])
    
def findfilms(content):
    pattern = r"(《.+?》)"
    return re.findall(pattern, content)

if __name__ == "__main__":
    main()
    input("press any key to continue...")
