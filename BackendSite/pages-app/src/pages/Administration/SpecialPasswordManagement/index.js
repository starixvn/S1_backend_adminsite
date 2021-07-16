import React, { useState, useEffect } from "react";
import ReactDOM from "react-dom";
import { api } from "../../../module/WebAPI";
import Result from "./Result";

const App = () => {
  const [site, setSite] = useState([]);
  const [opt, setOpt] = useState({
    siteId: 0,
    password:null
  });
  const [stateDatas, setDatas] = useState({
    Datas: null
  });
  const [isOpen,setisOpen] = useState(false);
  const [isEdit,setisEdit] = useState(false);
  let _options_site = "";
  useEffect(() => {
    api
      .post("/api/Common/AdminUserSiteList")
      .then(function (response) {
        if (response.data.errorCode === 0) {
          setSite(response.data === null ? [] : response.data.data);
        } else {
          alert(response.message);
        }
      })
      .catch(function (error) {
        alert(error);
      });
  }, []);

  const SetSite = (siteId) => {
    setOpt({
      siteId: siteId,
      password:null
    });
    let Data = {
        SiteId:siteId
    };
    setisOpen(false);
    setisEdit(false);
    api
      .post("/api/Admin/GetSpecialPasswordData", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
            if(res.data.data){
                setDatas({
                    Datas:res.data.data
                });
            }
            else{
                setDatas({
                    Datas:null
                });
            }
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };

  const Add =()=>{
    setisOpen(true)
  }
  const Cancel=()=>{
    setisOpen(false)
    setOpt({
        ...opt,
        password:null
    })
  }
  const AddSubmit=()=>{
    let Data = {
        SiteId:opt.siteId,
        Password:opt.password
    };

    api
      .post("/api/Admin/SetSpecialPasswordData", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
            setisOpen(false);
            Reset();
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  }

  if (site.length !== 0) {
    _options_site = site.map((s) => {
      return { value: s.siteId, displayText: s.siteName };
    });
  }

  const passwordEdit=(props)=>{
    setOpt({
        ...opt,
        password:props
    })
  }
  const EditClick =()=>{
    setisEdit(true);
  }
  const EditCancel =()=>{
    setOpt({
        ...opt,
        password:null
    })
    setisEdit(false);
  }
  const EditSubmit = ()=>{
    let Data = {
        SiteId:opt.siteId,
        Password:opt.password
    }; 
    api
      .post("/api/Admin/SetSpecialPasswordData", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
            setisEdit(false);
            Reset();
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });

  }

  const Reset =()=>{
    let Data = {
        SiteId:opt.siteId
    };
    api
      .post("/api/Admin/GetSpecialPasswordData", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
            if(res.data.data){
                setDatas({
                    Datas:res.data.data
                });
            }
            else{
                setDatas({
                    Datas:null
                });
            }
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  }

  return (
    <div className="container-fluid">
      <h4 className="page-name">Special Password Management</h4>
      <div
        className="filter"
        style={{
          position: "sticky",
          top: 0,
          left: 0,
          zIndex: 999,
          width: "100%",
          height: "90px",
        }}
      >
        {_options_site ? (
          <RadioBox
            Title="Available Sites"
            Options={_options_site}
            OnChange={SetSite}
            dataColumn="availableSites"
            Selected={opt.siteId}
          />
        ) : null}
      </div>
      {opt.siteId === 0 ? null : (
        <Result
          Datas={stateDatas.Datas}
          setisOpen={setisOpen}
          opt={opt}
          setOpt={setOpt}
          isOpen={isOpen}
          Add={Add}
          AddSubmit={AddSubmit}
          Cancel={Cancel}
          passwordEdit={passwordEdit}
          isEdit={isEdit}
          EditClick={EditClick}
          EditCancel={EditCancel}
          EditSubmit={EditSubmit}

        />
      )}
    </div>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

const RadioBox = ({
  Title,
  Options,
  Selected,
  OnChange,
  dataColumn,
  mainKey = null,
  disabled = false,
}) => {
  let opt = Options.map((o, oIdx) => {
    let key = `${Title}-${o.value}${mainKey === null ? "" : `-${mainKey}`}`;
    return (
      <RadioBtn
        key={key}
        Id={key}
        DisplayText={o.displayText}
        Value={`${o.value}${mainKey === null ? "" : `-${mainKey}`}`}
        dataColumn={dataColumn}
        Name={`${Title}${mainKey === null ? "" : `-${mainKey}`}`}
        OnChange={OnChange.bind(this, o.value, dataColumn, mainKey, oIdx)}
        Checked={Selected === o.value}
        Disabled={disabled}
      />
    );
  });
  return (
    <>
      <div className="row-flex">
        <div className="input-item">
          <label className="label">{Title}</label>
        </div>
      </div>
      <div className="row-flex">{opt}</div>
    </>
  );
};

const RadioBtn = ({
  Id,
  DisplayText,
  Value,
  Name,
  OnChange,
  Checked,
  Disabled,
}) => {
  return (
    <div className="custom-control custom-radio custom-control-inline">
      <input
        type="radio"
        id={Id}
        name={Name}
        className="custom-control-input revert-style"
        value={Value}
        onChange={OnChange}
        checked={Checked}
        disabled={Disabled}
      />
      <label className="custom-control-label" htmlFor={Id}>
        {DisplayText}
      </label>
    </div>
  );
};
