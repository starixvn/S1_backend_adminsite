import React, { useState, useEffect, useRef } from "react";
import { api } from "../../../module/WebAPI";
import WebAPI from "../../../module/WebAPI";
import { Editor } from "react-draft-wysiwyg";
import "react-draft-wysiwyg/dist/react-draft-wysiwyg.css";
import draftToHtml from "draftjs-to-html";
import htmlToDraft from "html-to-draftjs";
import Checkbox from "./Checkbox";
import { EditorState, convertToRaw, ContentState } from "draft-js";

const AccountStatus = {
  1: "Running",
  4: "Lock"
};

const defCurrencyType = {
  2: "MYR",
  4: "THB",
  13: "RMB",
  15: "IDR",
  51: "VND",
  61: "INR",
};

const DropDownBox = (props) => {
  const onChange = (e) => {
    if (props.onChange) {
      props.onChange(e.target.value, props.dataColumn);
    }
  };
  let optionRows = props.options.map((item) => {
    return (
        <option key={`${props.ColumnTitle}-${item.value}`} value={item.value}>
          {item.displayText}
        </option>
    );
  });
  
  return (
      <div className="input-item width-100" style={{ marginTop: "20px", marginBottom: "20px" }}>
        <span className="label">{props.columnTitle}</span>
        <div className="field">
          <select defaultValue={props.selectedValue} onChange={onChange}>
            {props.hasAll ? <option value="0">-</option> : null}
            {optionRows}
          </select>
        </div>
      </div>
  );
};

const EditPanel = (props) => {
  let defaultData = {
    optSite: "",
    accountstatus:0,
    currency:0,
    file: "",
    popupMessage: {},
    subject: {},
    content: {},
    batchId: "",
  };
  if (props.DetailDatas !== null && props.DetailDatas.message !== null) {
    let detailDatas = JSON.parse(props.DetailDatas.message);
    let Title = {},
      Content = {},
      PopupMessage = {};
    Object.keys(detailDatas).forEach((lang) => {
      if (detailDatas[lang]) {
        PopupMessage[lang] = detailDatas[lang].PopupMessage;
        Title[lang] = detailDatas[lang].Title;
        Content[lang] = detailDatas[lang].Content;
      }
    });
    defaultData.batchId = props.DetailDatas.batchId;
    defaultData.optSite = props.DetailDatas.siteId;
    defaultData.subject = Title;
    defaultData.content = Content;
    // console.log('props.DetailDatas.siteId = '+props.DetailDatas.siteId,'iLangList = '+props.SiteList[props.DetailDatas.siteId-1].iLangList);
    // setlangDatas( props.SiteList[props.DetailDatas.siteId-1].iLangList);
  }
  defaultData.accountstatus = props.filterInfo.AccountStatus;
  defaultData.currency = props.filterInfo.Currency;

  const [stateData, updateData] = useState(defaultData);
  const [stateSend, updateSend] = useState(false);
  const [sendData, updateSendData] = useState([]);
  const [SubmitStatus, setSubmitStatus] = useState(false);
  const [langDatas, setlangDatas] = useState(
    props.DetailDatas !== null
      ? props.SiteList[props.DetailDatas.siteId - 1].iLangList
      : null
  );
  const [popupState, setPopupState] = useState(false);
  const isMountedRef = useRef(null);
  
  useEffect(() => {
    isMountedRef.current = true;
    return () => (isMountedRef.current = false);
  });
  let _site = [];

  let allLang = ["en", "cs", "ch", "th", "id", "vn"];

  const EditFiled = (v, datacolumn, subDatas) => {
    if (subDatas && subDatas.lang) {
      // let aa = { ...stateData[datacolumn], [subDatas.lang]: v };
      updateData({
        ...stateData,
        [datacolumn]: {
          ...stateData[datacolumn],
          [subDatas.lang]: v,
        },
      });
      // console.log('1v = '+v,'datacolumn = '+datacolumn, 'subDatas = '+subDatas);
    } else {
      updateData({
        ...stateData,
        [datacolumn]: v,
      });
      //console.log('2v = '+v,'datacolumn = '+datacolumn, 'subDatas = '+subDatas);
      if (datacolumn === "optSite") {
        
        let getSiteItem = props.SiteList.find((x) => x.siteId === v);
        setlangDatas(getSiteItem.iLangList);
      }
    }
  };

  const SetMsg = () => {
    
    if (!stateSend) {
      if (isMountedRef.current) {
        updateSend(true);
      }
      
      let msg = "";
      let check = true;
      if (!stateData.optSite) {
        check = false;
        msg = "Please check Site.";
      } 
      else if ( (props.filterInfo.AccountStatus == 0 || props.filterInfo.Currency == 0 ) && !stateData.file) {
        check = false;
        msg = "Please choose AccountStatus / Currency  Or  upload excel file.";
      }
      else if (!stateData.subject || !stateData.subject.en) {
        check = false;
        msg = "Please input en subject.";
      } 
      else if (!stateData.content || !stateData.content.en) {
        check = false;
        msg = "Please input en content.";
      }
      else if (popupState &&  (!stateData.popupMessage || !stateData.popupMessage.en)) {
        check = false;
        msg = "Please input en pop up message.";
      }

      if (
        check &&
        window.confirm(
          "Are you sure to import data to " + _site[stateData.optSite] + "?"
        )
      ) {
        
        let msg = {};
        setSubmitStatus(true);
        defaultData.accountstatus = props.filterInfo.AccountStatus;
        defaultData.currency = props.filterInfo.Currency;
        
        Object.keys(stateData.popupMessage).forEach((sKey) => {
          if (stateData.popupMessage[sKey]) {
            if (msg[sKey]) {
              msg[sKey].PopupMessage = stateData.popupMessage[sKey];
            } else {
              msg[sKey] = {
                PopupMessage: stateData.popupMessage[sKey],
              };
            }
          }
        });

        Object.keys(stateData.subject).forEach((sKey) => {
          if (stateData.subject[sKey]) {
            if (msg[sKey]) {
              msg[sKey].Title = stateData.subject[sKey];
            } else {
              msg[sKey] = {
                Title: stateData.subject[sKey],
                Content: "",
              };
            }
          }
        });

        Object.keys(stateData.content).forEach((sKey) => {
          if (stateData.content[sKey]) {
            if (msg[sKey]) {
              msg[sKey].Content = stateData.content[sKey];
            } else {
              msg[sKey] = {
                Title: "",
                Content: stateData.content[sKey],
              };
            }
          }
        });
        
        let settings = { headers: { "content-type": "multipart/form-data" } };
        let file = document.getElementById("uploadfile").files[0];
        let formData = new FormData();
        formData.append("optSite", parseInt(stateData.optSite));
        formData.append("accountStatus", parseInt(defaultData.accountstatus));
        formData.append("currency", parseInt(defaultData.currency));
        formData.append("IsPopup", popupState);
        if (file != null){
          formData.append("excelUpload", file, file.name);
          if (langDatas != null) {
            langDatas.forEach((lang) => {
              if(msg[lang] == null){
                msg[lang] = {
                  PopupMessage: stateData.popupMessage.en,
                  Title: stateData.subject.en,
                  Content: stateData.content.en,
                };
              }
            });
          }
        }
        formData.append("personalMsg", JSON.stringify(msg));
        api
          .post("/api/Message/SendMessageBatch", formData, settings)
          .then(function (response) {
            setSubmitStatus(false);
            if (isMountedRef.current) {
              if (response.data) {
                let param = {
                  Url: "Message/ExportSendMessageBatchResultReport",
                  Data: {
                    BatchId: response.data.data,
                  },
                };
                props.filterInfo.AccountStatus = 0;
                props.filterInfo.Currency = 0;
                
                WebAPI.DownloadData(param);

                updateData({
                  optSite: "",
                  file: "",
                  popupMessage: {},
                  subject: {},
                  content: {},
                  batchId: "",
                });
                // updateSendData(response.data);
                props.SetEditMode(false);
                if (props.FilterInfo) props.FilterSubmit();
                alert("sent successfully");
              } else {
                alert("sent error");
              }
              updateSend(false);
            }
          })
          .catch(function (error) {
            updateSend(false);
            alert(error);
          });
      } else {
        updateSend(false);
        alert(msg);
      }
    }
  };

  const UpdateMsg = (id) => {
    if (!stateSend) {
      if (isMountedRef.current) {
        updateSend(true);
      }
      let msg = "";
      let check = true;

      if (!stateData.subject || !stateData.subject.en) {
        check = false;
        msg = "Please input en subject.";
      } else if (!stateData.content || !stateData.content.en) {
        check = false;
        msg = "Please input en content.";
      } else if (popupState &&  (!stateData.popupMessage || !stateData.popupMessage.en)) {
        check = false;
        msg = "Please input en pop up message.";
      }

      if (
        check &&
        window.confirm(
          "Are you sure to update data to " + _site[stateData.optSite] + "?"
        )
      ) {
        let msg = {};
        if (langDatas != null) {
          langDatas.forEach((lang) => {
            msg[lang] = {
              Title: stateData.subject.en,
              Content: stateData.content.en,
            };
          });
        }

        Object.keys(stateData.subject).forEach((sKey) => {
          if (stateData.subject[sKey]) {
            if (msg[sKey]) {
              msg[sKey].Title = stateData.subject[sKey];
            } else {
              msg[sKey] = {
                Title: stateData.subject[sKey],
                Content: "",
              };
            }
          }
        });

        Object.keys(stateData.content).forEach((sKey) => {
          if (stateData.content[sKey]) {
            if (msg[sKey]) {
              msg[sKey].Content = stateData.content[sKey];
            } else {
              msg[sKey] = {
                Title: "",
                Content: stateData.content[sKey],
              };
            }
          }
        });
        // let settings = { headers: { 'content-type': 'multipart/form-data' } }
        let formData = new FormData();
        formData.append("batchId", parseInt(stateData.batchId));
        formData.append("personalMsg", JSON.stringify(msg));
        api
          .post("/api/Message/UpdateBatchMessage", formData)
          // updateSend(true);
          // let Data = {
          //     batchId: id,
          //     personalMsg: JSON.stringify(msg)
          // };

          // api.post('/api/Message/UpdateBatchMessage', Data)
          .then(function (response) {
            if (isMountedRef.current) {
              if (response.data) {
                updateData({
                  optSite: "",
                  file: "",
                  popupMessage: {},
                  subject: {},
                  content: {},
                  batchId: "",
                });
                // updateSendData(response.data);
                props.SetEditMode(false);
                if (props.FilterInfo) props.FilterSubmit();
                alert("sent successfully");
              } else {
                alert("sent error");
              }
              updateSend(false);
            }
          })
          .catch(function (error) {
            updateSend(false);
            alert(error);
          });
      } else {
        updateSend(false);
        alert(msg);
      }
    }
  };
  
  let radio = "";
  if (props.SiteList === null) {
    return <></>;
  } else if (props.DetailDatas !== null) {
    radio = props.SiteList.map((value) => {
      _site[value.siteId] = value.siteName;
      return value.siteId == props.DetailDatas.siteId ? (
        <div
          key={`SiteOpt-${value.siteId}`}
          className="custom-control custom-radio custom-control-inline"
        >
          <input
            type="radio"
            id={`SiteId_${value.siteId}`}
            name="SiteList"
            className="custom-control-input"
            value={value.siteId}
            disabled={true}
            checked={true}
          />
          <label
            className="custom-control-label"
            htmlFor={`SiteId_${value.siteId}`}
          >
            {value.siteName}
          </label>
        </div>
      ) : null;
    });
  } else {
    radio = props.SiteList.map((value) => {
      _site[value.siteId] = value.siteName;
      return (
        <div
          key={`SiteOpt-${value.siteId}`}
          className="custom-control custom-radio custom-control-inline"
        >
          <input
            type="radio"
            id={`SiteId_${value.siteId}`}
            name="SiteList"
            className="custom-control-input"
            value={value.siteId}
            onChange={(e) => {
              EditFiled(value.siteId, "optSite");
            }}
            disabled={stateSend}
            checked={stateData.optSite === value.siteId}
          />
          <label
            className="custom-control-label"
            htmlFor={`SiteId_${value.siteId}`}
          >
            {value.siteName}
          </label>
        </div>
      );
    });
  }

  let langPanels =
    langDatas != null
      ? langDatas.map((lang) => {
          return (
            <OuterPanel
              key={`set-panel-${lang}`}
              HeaderContent={
                <span style={{ color: "white" }}>{`${lang}${
                  lang === "en" ? "  ( required )" : ""
                }`}</span>
              }
              Content={
                <SettingContent
                  popupState={popupState}
                  popupMessage={
                    stateData.popupMessage[lang] === undefined
                      ? ""
                      : stateData.popupMessage[lang]
                  }
                  subject={
                    stateData.subject[lang] === undefined
                      ? ""
                      : stateData.subject[lang]
                  }
                  content={
                    stateData.content[lang] === undefined
                      ? ""
                      : stateData.content[lang]
                  }
                  lang={lang}
                  EditFiled={EditFiled}
                />
              }
            />
          );
        })
      : null;
  let result = sendData.map((value, key) => {
    return <p key={`result-${key}`}>{value.message}</p>;
  });
  
  return (
    <>
      {radio}
      {props.DetailDatas === null ? (
        <>
          {/*<div className="filter" >*/}
            <div className="row-flex" style={{ display: "flex"}}>
              <label className="input-item width-100"> Method 1 :  </label>
              <DropDownBox
                  columnTitle="Account Status"
                  options={Object.keys(AccountStatus).map((key) => {
                    return { value: key, displayText: AccountStatus[key] };
                  })}
                  hasAll={true}
                  selectedValue={0}
                  dataColumn="AccountStatus"
                  onChange={props.onChange}
              />
              <DropDownBox
                  columnTitle="Currency"
                  options={Object.keys(defCurrencyType).map((key) => {
                    return { value: key, displayText: defCurrencyType[key] };
                  })}
                  hasAll={true}
                  selectedValue={0}
                  dataColumn="Currency"
                  onChange={props.onChange}
              />
            </div>
          {/*</div>*/}
          <div className="row-flex" style={{ display: "flex"}}>
            <label className="input-item width-100"> Method 2 :  </label>
              <div className="form-group width-200" >
                <label className="col-form-label d-block">
                  Download Template File{" "}
                </label>
                <a
                  className="btn btn-danger"
                  role="button"
                  href="../../../resource/examplefile/PersonalMessageBatch.xlsx"
                  download="PersonalMessageTemplateFile.xlsx"
                >
                  Template File
                </a>
              </div>
              <div className="form-group width-200" >
                <label className="col-form-label">Upload File</label>
                <input
                  id="uploadfile"
                  className="form-control"
                  type="file"
                  accept="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet, application/vnd.ms-excel"
                  onChange={(e) => {
                    EditFiled(e.target.value, "file");
                  }}
                  disabled={stateSend}
                  value={stateData.file}
                />
              </div>
          </div>
        </>
      ) : null}
      {langDatas != null ? <Checkbox
          title="Pop Up"
          value="popup"
          checked={popupState}
          toggle = {setPopupState}
       /> : null}
      {langPanels}
      {props.DetailDatas === null ? (
        <button
          className="btn btn-add"
          style={{ marginTop: "20px", marginBottom: "40px", float: "right" }}
          onClick={SetMsg}
          disabled={SubmitStatus}
        >
          Submit
        </button>
      ) : (
        <button
          className="btn btn-add"
          style={{ marginTop: "20px", marginBottom: "40px", float: "right" }}
          onClick={UpdateMsg}
        >
          Update
        </button>
      )}

      <button
        className="btn btn-cancel"
        style={{
          marginTop: "20px",
          marginBottom: "40px",
          marginRight: "10px",
          float: "right",
        }}
        onClick={props.SetEditMode.bind(null, false)}
      >
        Back
      </button>
      <div className="form-group">{result}</div>
    </>
  );
};

const OuterPanel = (props) => {
  return (
    <div className="panel level-1 mt-20">
      <a style={{ justifyContent: "space-between" }} className="panel-title">
        {props.HeaderContent}
      </a>
      <div className="panel-body">
        <div className="collapse show">{props.Content}</div>
      </div>
    </div>
  );
};

const SettingContent = (props) => {
  const onEditorStateChange = (editorState) => {
    let editorStatehtml = draftToHtml(
      convertToRaw(editorState.getCurrentContent())
    );
    props.EditFiled(editorStatehtml, "content", { lang: props.lang });
  };
  let content = EditorState.createEmpty();
  let contentBlock = htmlToDraft(props.content);
  if (contentBlock) {
    let contentstate = ContentState.createFromBlockArray(
      contentBlock.contentBlocks
    );
    content = EditorState.createWithContent(contentstate);
  }
  return (
    <div className="panel-body">
      {props.popupState ? 
      <div className="form-group">
        <label>Pop up Message</label>
        <input
          type="text"
          className="form-control"
          value={props.popupMessage}
          onChange={(e) => {
            props.EditFiled(e.target.value, "popupMessage", { lang: props.lang });
          }}
        />
      </div> : null }
      <div className="form-group">
        <label>Subject</label>
        <input
          type="text"
          className="form-control"
          value={props.subject}
          onChange={(e) => {
            props.EditFiled(e.target.value, "subject", { lang: props.lang });
          }}
        />
      </div>
      <div className="form-group">
        <label>Content</label>
        {/* <textarea
          className="form-control"
          rows="8"
          value={props.content}
          onChange={(e) => {
            props.EditFiled(e.target.value, "content", { lang: props.lang });
          }}
        ></textarea> */}
        <Editor
          defaultEditorState={content}
          // toolbarClassName="toolbar-class"
          toolbar={
            {
              options: ['inline', 'blockType', 'fontSize', 'fontFamily','list','textAlign'
              , 'colorPicker','link', 'emoji', 'image', 'remove', 'history'],
              blockType: {
                inDropdown: true,
                options: ['Normal', 'H1', 'H2', 'H3', 'H4', 'H5', 'H6'],
                className: undefined,
                component: undefined,
                dropdownClassName: undefined,
              },
              inline: {
                options: ['bold','italic','underline','strikethrough'],
                bold: { className: 'demo-option-custom' },
                italic: { className: 'demo-option-custom' },
                underline: { className: 'demo-option-custom' },
                strikethrough: {className: 'demo-option-custom' },
                monospace: { className: 'demo-option-custom' },
                superscript: {className: 'demo-option-custom'},
                subscript: { className: 'demo-option-custom' }
              },
              textAlign: {
                inDropdown: false,
                className: undefined,
                component: undefined,
                dropdownClassName: undefined,
                options: ['left', 'center', 'right'],
              },
              list: {
                inDropdown: false,
                className: undefined,
                component: undefined,
                dropdownClassName: undefined,
                options: ['unordered', 'ordered'],
              }
            }
          }
          wrapperClassName="wrapper-class"
          editorClassName="editor-class"
          onEditorStateChange={onEditorStateChange}
        />
      </div>
    </div>
  );
};

export default EditPanel;
