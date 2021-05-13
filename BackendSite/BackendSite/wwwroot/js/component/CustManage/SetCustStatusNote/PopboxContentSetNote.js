import React from "react";

export default ({ NewDatas, Action }) => {
  return (
    <div className="newwrapper">
      <div className="popup-wrap is-show">
        <div className="popup is-open">
          <div className="popup__head">
            <span className="text">Reason</span>
            <div className="newbtn">
              <i className="icon icon-close"></i>
            </div>
          </div>
          <div className="popup__body os-host os-theme-dark os-host-resize-disabled os-host-scrollbar-horizontal-hidden os-host-scrollbar-vertical-hidden os-host-transition">
            <div className="os-resize-observer-host">
              <div className="os-resize-observer observed"></div>
            </div>
            <div className="os-size-auto-observer">
              <div className="os-resize-observer observed"></div>
            </div>
            <div className="os-content-glue"></div>
            <div className="os-padding">
              <div className="os-viewport os-viewport-native-scrollbars-invisible">
                <div className="os-content">
                  <div className="popup__row">
                    <div className="form">
                      <div className="form__group">
                        <div className="form__title">
                          <span className="text">Reason</span>
                        </div>
                        <div className="textfield">
                          <InputBox
                            Title="Reason"
                            HideTitle={true}
                            OnChange={Action.EditReason}
                            dataColumn="reason"
                            value={NewDatas.reason}
                            inputClass="textfield__input"
                          />
                        </div>
                      </div>
                    </div>
                  </div>
                  <div className="popup__row">
                    <div className="newbtn-bar">
                      <div className="newbtn-bar__group">
                        <div
                          className="newbtn newbtn-primary newbtn-w-min"
                          onClick={Action.Submit}
                        >
                          <span className="text">Submit</span>
                        </div>
                        <div
                          className="newbtn newbtn-secondary newbtn-w-min"
                          onClick={Action.Cancel}
                        >
                          <span className="text">Cancel</span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div className="os-scrollbar os-scrollbar-horizontal os-scrollbar-unusable">
              <div className="os-scrollbar-track os-scrollbar-track-off">
                <div className="os-scrollbar-handle"></div>
              </div>
            </div>
            <div className="os-scrollbar os-scrollbar-vertical os-scrollbar-unusable">
              <div className="os-scrollbar-track os-scrollbar-track-off">
                <div className="os-scrollbar-handle"></div>
              </div>
            </div>
            <div className="os-scrollbar-corner"></div>
          </div>
        </div>
      </div>
    </div>
  );
};

const InputBox = (props) => {
  const OnChange = (e) => {
    let value = e.target.value;
    let checked =
      value === "" || props.regex === undefined
        ? true
        : value.match(props.regex);
    if (checked) {
      props.OnChange(value, props.dataColumn);
    }
  };
  return (
    <div className={props.ClassName ? props.ClassName : "input-item"}>
      {props.HideTitle === true ? null : (
        <label className="label">{props.Title}</label>
      )}
      <div
        className={`field${props.iptSubClass ? " " + props.iptSubClass : ""}`}
      >
        {props.SubTitle ? (
          <span className="bule-bold">{props.SubTitle}</span>
        ) : null}
        <input
          type={props.type ? props.type : "text"}
          value={
            props.value === null || props.value === undefined ? "" : props.value
          }
          onChange={OnChange}
          disabled={props.Disabled === true}
          className={props.inputClass ? props.inputClass : ""}
        />
      </div>
    </div>
  );
};
