package main

import (
	"encoding/json"
	"fmt"
	"io/ioutil"
	"log"
	"net/http"
	"os"
	"regexp"
	"strings"
)

var source = "https://www.zhihu.com/api/v4/questions/58498720/answers?include=content&limit=1&offset=0"

func main() {
	data, err := get(source)
	if err != nil {
		panic(err)
	}

	content, err := jsonContent(data)
	if err != nil {
		panic(err)
	}

	cups := findCupMap([]byte(content))
	cupImageCount := make(map[string]int)

	for cup, answers := range cups {
		for _, singleAnswer := range answers {
			imageLinks, err := findLinks(singleAnswer)
			if err != nil {
				log.Println(err, singleAnswer)
				continue
			}

			if len(imageLinks) < 1 {
				continue
			}

			successCount := downloadImages(cup+"/", cupImageCount[cup], imageLinks)
			cupImageCount[cup] += successCount
			log.Printf("OK ... %s .. %2d images, link = %s\n", cup, successCount, singleAnswer)
		}
	}
}

func findLinks(singleAnswerURL string) ([]string, error) {
	data, err := get(singleAnswerURL)
	if err != nil {
		return nil, err
	}

	reg, _ := regexp.Compile("<span class=\"RichText ztext CopyrightRichText-richText\".*?</span>")
	result := reg.FindString(string(data))

	reg, _ = regexp.Compile("<img src=\"https://.*?.jpg\"")
	imgs := reg.FindAll([]byte(result), -1)

	// log.Printf("[find] len(data) = %d, len(result) = %5d, len(imgs) = %2d\n", len(data), len(result), len(imgs))

	links := make([]string, 0, len(imgs)/2)
	for i := range imgs {
		temp := string(imgs[i])
		i1, i2 := strings.Index(temp, "\""), strings.LastIndex(temp, "\"")
		temp = temp[i1+1 : i2]
		if !contains(links, temp) {
			links = append(links, temp)
		}
	}

	return links, nil
}

func get(url string) ([]byte, error) {
	req, err := http.NewRequest("GET", url, nil)
	if err != nil {
		return nil, err
	}

	// req.Header.Add("Accept", "*/*")
	// req.Header.Add("Accept-Language", "en-US,en;q=0.8")
	// req.Header.Add("Cache-Control", "max-age=0")
	// req.Header.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36")
	// req.Header.Add("Connection", "keep-alive")

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		return nil, err
	}

	return ioutil.ReadAll(resp.Body)
}

func downloadImages(foldername string, offset int, links []string) int {
	_, err := os.Stat(foldername)
	if os.IsNotExist(err) {
		os.MkdirAll(foldername, 0755)
	}
	if !strings.HasSuffix(foldername, "/") {
		foldername += "/"
	}

	successCount := 0
	for _, link := range links {
		data, err := get(link)
		if err != nil {
			log.Println(foldername, link, err)
			continue
		}

		filename := fmt.Sprintf("%s%d.jpg", foldername, successCount+offset)
		ioutil.WriteFile(filename, data, 0644)
		// ioutil.WriteFile(filename[:len(filename)-3]+"txt", []byte(link), 0644)
		successCount++
	}

	return successCount
}

func jsonContent(data []byte) (string, error) {
	var x struct {
		Data []struct {
			Content string `json:"content"`
		} `json:"data"`
	}

	if err := json.Unmarshal(data, &x); err != nil {
		return "", err
	}

	return x.Data[0].Content, nil
}

func findCupMap(data []byte) map[string][]string {
	var (
		maps        = make(map[string][]string)
		currentCup  = "default"
		labelreg, _ = regexp.Compile("<(a.*?|p)>.*?</(a|p)>")
		linkreg, _  = regexp.Compile("\"https://\\S+\"")
	)

	labels := labelreg.FindAll(data, -1)

	for i := range labels {
		temp := string(labels[i])

		if strings.HasPrefix(temp, "<p>") && strings.Index(temp, "cup") > 0 {
			i1, i2 := strings.Index(temp, ">"), strings.Index(temp, "cup")
			currentCup = strings.TrimSpace(temp[i1+1 : i2])

		} else if strings.HasPrefix(temp, "<a href=") {
			if maps[currentCup] == nil {
				maps[currentCup] = make([]string, 0, 1)
			}
			link := linkreg.FindString(temp)
			maps[currentCup] = append(maps[currentCup], link[1:len(link)-1])
		}
	}

	return maps
}

func contains(list []string, item string) bool {
	for _, each := range list {
		if each == item {
			return true
		}
	}
	return false
}
