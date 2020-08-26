import React, { useState, useEffect } from "react";
import WebAPI, { api } from '../../../module/WebAPI';
import qs from 'qs';

const EditPanel = () => {
    const [stateData, updateData] = useState({
        optSite: '',
        file: '',
        subject: {},
        content: {}
    });
    const [site, setSite] = useState([]);
    const [stateSend, updateSend] = useState(false);
    const [sendData, updateSendData] = useState([]);
    let _site = [];

    let allLang = ['en', 'cs', 'ch', 'th', 'id', 'vn'];

    useEffect(() => {
        let param = {
            Url: 'Common/AdminUserSiteList'
        };
        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {
                setSite(res.data === null ? [] : res.data);
            } else {
                // alert(res.message);
            }
        });
    }, []);

    const EditFiled = (v, datacolumn, subDatas) => {
        if (subDatas && subDatas.lang) {
            updateData({
                ...stateData,
                [datacolumn]: {
                    ...stateData[datacolumn],
                    [subDatas.lang]: v
                }
            });
        } else {
            updateData({
                ...stateData,
                [datacolumn]: v
            });
        }
    }

    const SetMsg = () => {
        let msg = "";
        let check = true;
        if (!stateData.optSite) { check = false; msg = "Please check Site."; }
        else if (!stateData.file) { check = false; msg = "Please choose upload excel file."; }
        else if (!stateData.subject || !stateData.subject.en) {
            check = false;
            msg = "Please input en subject.";
        } else if (!stateData.content || !stateData.content.en) {
            check = false;
            msg = "Please input en content.";
        }

        if (check && window.confirm("Are you sure to import data to " + _site[stateData.optSite] + "?")) {
            let msg = {};
            allLang.forEach(lang => {
                msg[lang] = {
                    Title: stateData.subject.en,
                    Content: stateData.content.en
                }
            });

            Object.keys(stateData.subject).forEach(sKey => {
                if (stateData.subject[sKey]) {
                    if (msg[sKey]) {
                        msg[sKey].Title = stateData.subject[sKey];
                    } else {
                        msg[sKey] = {
                            Title: stateData.subject[sKey],
                            Content: ''
                        }
                    }
                }
            });

            Object.keys(stateData.content).forEach(sKey => {
                if (stateData.content[sKey]) {
                    if (msg[sKey]) {
                        msg[sKey].Content = stateData.content[sKey];
                    } else {
                        msg[sKey] = {
                            Title: '',
                            Content: stateData.content[sKey]
                        }
                    }
                }
            });


            let settings = { headers: { 'content-type': 'multipart/form-data' } }
            let file = document.getElementById("uploadfile").files[0];
            let formData = new FormData();
            formData.append('optSite', parseInt(stateData.optSite));
            formData.append("excelUpload", file, file.name);
            formData.append('personalMsg', JSON.stringify(msg));
            if (!stateSend) {
                updateSend(true);
                api.post('/api/Message/SendMessageBatch', formData, settings)
                    .then(function (response) {
                        updateSend(false);
                        if (response.data) {
                            updateData({
                                optSite: '',
                                file: '',
                                subject: {},
                                content: {}
                            });
                            updateSendData(response.data);
                            alert('sent successfully');
                        } else {
                            alert("sent error");
                        }
                    }).catch(function (error) {
                        updateSend(false);
                        alert(error);
                    })
            }
        } else {
            alert(msg);
        }
    }

    let radio = "";
    if (site.length === 0) {
        return <></>
    } else {
        radio = site.data.map((value) => {
            _site[value.siteId] = value.siteName;
            return (
                <div key={`SiteOpt-${value.siteId}`} className="custom-control custom-radio custom-control-inline">
                    <input type="radio" id={`SiteId_${value.siteId}`} name="SiteList" className="custom-control-input" value={value.siteId} onChange={(e) => { EditFiled(value.siteId, 'optSite') }} disabled={stateSend} checked={stateData.optSite === value.siteId} />
                    <label className="custom-control-label" htmlFor={`SiteId_${value.siteId}`}>{value.siteName}</label>
                </div>
            )
        });
    }

    let langPanels = allLang.map(lang => {
        return (
            <OuterPanel
                key={`set-panel-${lang}`}
                HeaderContent={<span style={{ color: 'white' }}>{`${lang}${lang === 'en' ? '  ( required )' : ''}`}</span>}
                Content={<SettingContent subject={stateData.subject[lang] === undefined ? '' : stateData.subject[lang]} content={stateData.content[lang] === undefined ? '' : stateData.content[lang]} lang={lang} EditFiled={EditFiled} />}
            />
        );
    });
    let result = sendData.map((value, key) => { return <p key={`result-${key}`}>{value.message}</p> })
    return (
        <>
            {radio}
            <div className="form-group">
                <label className="col-form-label d-block">Download Template File </label>
                <a className="btn btn-danger" role="button" href="../../../resource/examplefile/PersonalMessageBatch.xlsx" download="PersonalMessageTemplateFile.xlsx">Template File</a>
            </div>
            <div className="form-group width-200">
                <label className="col-form-label">Upload File</label>
                <input id="uploadfile" className="form-control" type="file" accept="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet, application/vnd.ms-excel" onChange={(e) => { EditFiled(e.target.value, 'file') }} disabled={stateSend} value={stateData.file} />
            </div>
            {langPanels}
            <button className="btn btn-add" style={{ marginTop: '20px', marginBottom: '40px', float: 'right' }} onClick={SetMsg}>Submit</button>
            <div className="form-group">{result}</div>
        </>
    );
}

const OuterPanel = (props) => {
    return (
        <div className="panel level-1 mt-20">
            <a style={{ "justifyContent": "space-between" }} className="panel-title">
                {props.HeaderContent}
            </a>
            <div className="panel-body">
                <div className="collapse show">
                    {props.Content}
                </div>
            </div>
        </div>
    )
}

const SettingContent = (props) => {
    return (
        <div className="panel-body">
            <div className="form-group">
                <label>Subject</label>
                <input type="text" className="form-control" value={props.subject} onChange={(e) => { props.EditFiled(e.target.value, 'subject', { lang: props.lang }) }} />
            </div>
            <div className="form-group">
                <label>Content</label>
                <textarea className="form-control" rows="8" value={props.content} onChange={(e) => { props.EditFiled(e.target.value, 'content', { lang: props.lang }) }}></textarea>
            </div>
        </div>
    )
}

export default EditPanel;