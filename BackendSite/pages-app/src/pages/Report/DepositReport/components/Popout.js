import React from "react";

const Modal = ({ handleClose, show, accountName,accountHeaderRows,dateGroupHeaderRows,dateHeaderRows,dateBodyRows,dateTotalRows }) => {
  
    const showHideStyle = show ? {display:"block",background:"rgba(0, 0, 0, 0.6)"} : {};

  return (
    <div className="modal" style={showHideStyle}>
      <div style={{position:"absolute",width:"90%",height:"90%",background:"#ffffff",left:"5%",top:"5%",padding:"10px"}}>
            <div style={{background: "#E9E9E9",borderBottom: "0",padding:"8px",display:"flex",border:"1px solid #d5d5d5",height:"49px"}}>
                <span style={{fontWeight:"bold"}}>This is Account <span style={{color:"red"}}>{accountName}</span> data by date.</span>
                <button type="button" className="btn btn-approved" style={{position:"absolute",right:"31px"}} onClick={handleClose}>Close</button>
            </div>
            <div style={{overflow:"scroll",height:"85%"}}>
                <table className="table table-striped">
                    <thead>
                        <tr>
                            <th style={{ position: "sticky", top: "0",left:"0",height:"36.8px",zIndex:"3"}}>Account</th>
                            {accountHeaderRows}
                        </tr>
                        <tr>
                            <th style={{ position: "sticky", top: "36.8px",left:"0",height:"36.8px",zIndex:"3", background:"#FFFF45",color:"#F00000"}}>ByDate</th>
                            {dateGroupHeaderRows}
                        </tr>
                        <tr>
                            {dateHeaderRows}
                        </tr>
                    </thead>
                    <tbody>
                        {dateBodyRows}
                        {dateTotalRows}
                    </tbody>
                </table>
            </div>
        </div>
    </div>
  );
};

export default Modal;
