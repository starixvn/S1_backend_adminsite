import React from "react";
import ReactJsonView from "react-json-view";

const PopBoxContent = (props) => {
    return (
        <div className="modal" style={props.ShowInfoStatus
            ? { display: "block", background: "rgba(0, 0, 0, 0.6)" } : {}}>
            <div style={{
                position: "absolute",
                width: "60%",
                height: "60%",
                background: "#ffffff",
                left: "20%",
                top: "15%",
                padding: "10px",
            }}>
                <div style={{
                    background: "#E9E9E9",
                    borderBottom: "0",
                    padding: "8px",
                    display: "flex",
                    border: "1px solid #d5d5d5",
                    height: "49px",
                }}>
                    <h3>Info</h3>
                    <button type="button"
                        className="btn btn-approved"
                        style={{ position: "absolute", right: "31px" }}
                        onClick={props.CloseInfo}>
                        Close
                    </button>
                </div>
                <div style={{ overflow: "scroll", height: "87%" }}>
                    {<ReactJsonView
                        src={props.Info}
                        theme="rjv-default"
                        iconStyle="square"
                        displayDataTypes={false}
                    />}
                </div>
            </div>
        </div>
    )
}

export default PopBoxContent;